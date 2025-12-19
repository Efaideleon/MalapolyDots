using DOTS.Characters;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public partial struct PropertyBlinkSystem : ISystem
    {
        private ComponentLookup<PropertySpaceTag> propertyLookup;
        private ComponentLookup<BlinkingFlagMaterialOverride> blinkingMaterialLookup;
        private ComponentLookup<GameStateComponent> gameStateLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<BlinkingFlagMaterialOverride>();
            state.RequireForUpdate<PropertySpaceTag>();

            blinkingMaterialLookup = SystemAPI.GetComponentLookup<BlinkingFlagMaterialOverride>(true);
            propertyLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>(true);
            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>(true);

            state.EntityManager.CreateSingleton(new PreviousPropertyLandedOn { Entity = Entity.Null });
        }

        public void OnUpdate(ref SystemState state)
        {
            propertyLookup.Update(ref state);
            blinkingMaterialLookup.Update(ref state);
            gameStateLookup.Update(ref state);

            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var spaceLandedOn = SystemAPI.GetComponent<SpaceLandedOn>(activePlayer);
            var arrived = SystemAPI.GetComponent<FinalArrived>(activePlayer);

            // Player arrived at a property.
            if (arrived.Value && propertyLookup.HasComponent(spaceLandedOn.entity))
            {
                if (blinkingMaterialLookup.HasComponent(spaceLandedOn.entity))
                {
                    var blinking = blinkingMaterialLookup.GetRefRW(spaceLandedOn.entity);
                    blinking.ValueRW.Value = 1f;
                    SystemAPI.GetSingletonRW<PreviousPropertyLandedOn>().ValueRW.Entity = spaceLandedOn.entity;
                }
            }

            // Player is moving
            var gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            if (gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion))
            {
                // Disable blinking.
                var previousPropertyEntity = SystemAPI.GetSingleton<PreviousPropertyLandedOn>().Entity;
                if (previousPropertyEntity != Entity.Null)
                {
                    blinkingMaterialLookup.GetRefRW(previousPropertyEntity).ValueRW.Value = 0f;
                }
            }
        }
    }

    public struct PreviousPropertyLandedOn : IComponentData
    {
        public Entity Entity;
    }
}
