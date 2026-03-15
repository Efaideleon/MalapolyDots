using Assets.Scripts.DOTS.Characters;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;

namespace DOTS.GamePlay
{
    // TODO: This has to go in the server simulation.
    //[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [BurstCompile]
    public partial struct RayCastSystem : ISystem
    {
        const uint floorLayerBitMask = 1u << 8;
        const uint ignoreFloorLayerBitMask = ~(1u << 8);

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<HitCastResult>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Run every time the touch position is pressed.
            NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
            foreach (var (touchPosition, rayCastData, hitCastResult)  in 
                    SystemAPI.Query<RefRO<TouchPositionInput>, RefRO<TouchRayCastDataInput>, RefRW<HitCastResult>>().WithAll<Simulate>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    if (touchPosition.ValueRO.IsHeld.IsSet)
                    {
                        var ray = rayCastData.ValueRO;
                        UnityEngine.Debug.Log($"[RayCastSystem] | position: {touchPosition.ValueRO.Position} , rayCastData : {ray.RayOrigin} {state.World}");
                        PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
                        var collisionWorld = world.CollisionWorld;

                        RaycastInput floorRayInput = CreateRaycastInput(ray.RayOrigin, ray.RayEnd, floorLayerBitMask);
                        collisionWorld.CastRay(floorRayInput, out RaycastHit floorHit);

                        RaycastInput objectRayInput = CreateRaycastInput(ray.RayOrigin, ray.RayEnd, ignoreFloorLayerBitMask);
                        collisionWorld.CastRay(objectRayInput, out RaycastHit objectHit);

                        // How do we that the info in this raycast is only set for the local client?
                        hitCastResult.ValueRW.FloorHit = new HitData { Entity = floorHit.Entity, Position = floorHit.Position };
                        hitCastResult.ValueRW.ObjectHit = new HitData { Entity = objectHit.Entity, Position = objectHit.Position };
                    }
                }
            }
        }

        [BurstCompile]
        public readonly RaycastInput CreateRaycastInput(float3 origin, float3 end, uint collisionBitMask)
        {
            RaycastInput input = new()
            {
                Start = origin,
                End = end,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = collisionBitMask,
                    GroupIndex = 0
                }
            };

            return input;
        }
    }
}
