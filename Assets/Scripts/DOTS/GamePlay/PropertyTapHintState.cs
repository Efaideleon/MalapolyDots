using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public struct PropertyTapHintState : IComponentData
    {
        public Entity Property;
        public bool Hovered, Learned, CanPrompt;
        public uint LastTapSequence;

        public void ObserveTap(LocalPropertyTapResult tap)
        {
            if (tap.Sequence == 0 || tap.Sequence == LastTapSequence) return;
            LastTapSequence = tap.Sequence;
            if (!tap.BlockedByUI && Property != Entity.Null && tap.Property == Property) Learned = true;
        }
    }

    public static class PropertyTapHintRules
    {
        public static EntityQuery CreateCollidersQuery(EntityManager manager) =>
            manager.CreateEntityQuery(typeof(PropertySpaceTag), typeof(PhysicsCollider), typeof(LocalToWorld));

        public static bool IsHovered(EntityManager manager, EntityQuery colliders, Entity property, float3 origin, float3 end)
        {
            Entity nearest = Entity.Null;
            float fraction = float.MaxValue;
            using var entities = colliders.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
                if (RayCastSystem.CastPropertyRay(manager.GetComponentData<PhysicsCollider>(entity),
                    manager.GetComponentData<LocalToWorld>(entity).Value, origin, end, out var hit) && hit.Fraction < fraction)
                {
                    nearest = entity;
                    fraction = hit.Fraction;
                }
            return property != Entity.Null && nearest == property;
        }

        public static Entity EligibleProperty(EntityManager manager, GameStateComponent game,
            Entity player, int localId, bool hasDebt)
        {
            if (!game.AllPlacesInstantiated || hasDebt || localId <= 0 ||
                !manager.HasComponent<GhostOwner>(player) ||
                manager.GetComponentData<GhostOwner>(player).NetworkId != localId ||
                !manager.HasComponent<PlayerMovementState>(player) ||
                !manager.HasComponent<SpaceLandedOn>(player)) return Entity.Null;
            if (manager.HasComponent<BankruptPlayer>(player) && manager.GetComponentData<BankruptPlayer>(player).Value)
                return Entity.Null;
            var property = manager.GetComponentData<SpaceLandedOn>(player).entity;
            return manager.HasComponent<PropertySpaceTag>(property) &&
                PropertyActionRules.CanOpen(game.State, manager.GetComponentData<PlayerMovementState>(player).Value,
                    property, property) ? property : Entity.Null;
        }
    }
}
