using Assets.Scripts.DOTS.Characters;
using DOTS.GameSpaces;
using Input;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public struct LocalPropertyTapResult : IComponentData
    {
        public uint Sequence;
        public bool BlockedByUI;
        public Entity Property;
        public float3 Position;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct RayCastSystem : ISystem
    {
        private uint processedSequence;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalPropertyTap>();
            state.EntityManager.CreateSingleton<LocalPropertyTapResult>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tap = SystemAPI.GetSingleton<LocalPropertyTap>();
            if (tap.Sequence == 0 || tap.Sequence == processedSequence) return;
            processedSequence = tap.Sequence;
            if (tap.BlockedByUI)
            {
                SystemAPI.SetSingleton(new LocalPropertyTapResult { Sequence = tap.Sequence, BlockedByUI = true });
                UnityEngine.Debug.Log("[PropertyTap] UI handled this tap; property picking skipped.");
                return;
            }
            var ray = tap;
            var nearest = new HitData { Entity = Entity.Null };
            float nearestFraction = float.MaxValue;
            int colliderCount = 0;

            // Use the rendered property transforms and actual baked collider geometry.
            // UI picking must not depend on a predicted physics broadphase being rebuilt.
            foreach (var (collider, transform, entity) in
                SystemAPI.Query<RefRO<PhysicsCollider>, RefRO<LocalToWorld>>()
                    .WithAll<PropertySpaceTag>().WithEntityAccess())
            {
                if (!collider.ValueRO.Value.IsCreated) continue;
                colliderCount++;
                if (CastPropertyRay(collider.ValueRO, transform.ValueRO.Value,
                        ray.RayOrigin, ray.RayEnd, out var hit) && hit.Fraction < nearestFraction)
                {
                    nearestFraction = hit.Fraction;
                    nearest = new HitData
                    {
                        Entity = entity,
                        Position = math.lerp(ray.RayOrigin, ray.RayEnd, hit.Fraction)
                    };
                }
            }

            SystemAPI.SetSingleton(new LocalPropertyTapResult
            {
                Sequence = tap.Sequence,
                Property = nearest.Entity,
                Position = nearest.Position
            });
            if (nearest.Entity == Entity.Null)
                UnityEngine.Debug.Log($"[PropertyTap] Ray missed {colliderCount} property colliders. Origin={ray.RayOrigin}, end={ray.RayEnd}.");
        }

        public static bool CastPropertyRay(PhysicsCollider collider, float4x4 localToWorld,
            float3 origin, float3 end, out RaycastHit hit)
        {
            hit = default;
            if (!collider.Value.IsCreated || !math.all(math.isfinite(origin)) ||
                !math.all(math.isfinite(end)) || math.lengthsq(end - origin) < 0.0001f ||
                math.abs(math.determinant(localToWorld)) < 0.000001f) return false;

            var inverse = math.inverse(localToWorld);
            var filter = collider.Value.Value.GetCollisionFilter();
            // Match this collider's categories, independently of the board's floor mask.
            var input = new RaycastInput
            {
                Start = math.transform(inverse, origin),
                End = math.transform(inverse, end),
                Filter = new CollisionFilter
                {
                    BelongsTo = filter.CollidesWith,
                    CollidesWith = filter.BelongsTo,
                    GroupIndex = filter.GroupIndex > 0 ? filter.GroupIndex : 0
                }
            };
            return collider.Value.Value.CastRay(input, out hit);
        }

        public readonly RaycastInput CreateRaycastInput(float3 origin, float3 end, uint collisionBitMask)
        {
            return new RaycastInput
            {
                Start = origin,
                End = end,
                Filter = new CollisionFilter { BelongsTo = ~0u, CollidesWith = collisionBitMask }
            };
        }
    }
}
