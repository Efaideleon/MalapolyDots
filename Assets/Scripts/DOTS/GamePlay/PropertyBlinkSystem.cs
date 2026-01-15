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
        private BufferLookup<PlayerArrivedAtDestinationEvent> playerArrivedBufferLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<BlinkingFlagMaterialOverride>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<PlayerArrivedAtDestinationEvent>();

            blinkingMaterialLookup = SystemAPI.GetComponentLookup<BlinkingFlagMaterialOverride>();
            propertyLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>(true);
            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>(true);
            playerArrivedBufferLookup = SystemAPI.GetBufferLookup<PlayerArrivedAtDestinationEvent>(true);

            state.EntityManager.CreateSingleton(new PreviousPropertyLandedOn { Entity = Entity.Null });
        }

        public void OnUpdate(ref SystemState state)
        {
            var lastSystemVersion = state.LastSystemVersion;
            propertyLookup.Update(ref state);
            blinkingMaterialLookup.Update(ref state);
            gameStateLookup.Update(ref state);
            playerArrivedBufferLookup.Update(ref state);

            var arrivedBufferEntity = SystemAPI.GetSingletonEntity<PlayerArrivedAtDestinationEvent>();
            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var spaceLandedOn = SystemAPI.GetComponent<SpaceLandedOn>(activePlayer);

            // Player arrived at a property.
            if (playerArrivedBufferLookup.DidChange(arrivedBufferEntity, lastSystemVersion))
            {
                UnityEngine.Debug.Log($"[PropertyBlinkSystem] | arrived on property!");
                if(propertyLookup.HasComponent(spaceLandedOn.entity))
                {
                    if (blinkingMaterialLookup.HasComponent(spaceLandedOn.entity))
                    {
                        var blinking = blinkingMaterialLookup.GetRefRW(spaceLandedOn.entity);
                        blinking.ValueRW.Value = 1f;
                        UnityEngine.Debug.Log($"[PropertyBlinkSystem] | turning blinking on!");
                        SystemAPI.GetSingletonRW<PreviousPropertyLandedOn>().ValueRW.Entity = spaceLandedOn.entity;
                    }
                }
            }

            // Player is moving
            var gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            if (gameStateLookup.DidChange(gameStateEntity, lastSystemVersion))
            {
                // Disable blinking.
                var previousPropertyEntity = SystemAPI.GetSingleton<PreviousPropertyLandedOn>().Entity;
                if (previousPropertyEntity != Entity.Null)
                {
                    blinkingMaterialLookup.GetRefRW(previousPropertyEntity).ValueRW.Value = 0f;
                    UnityEngine.Debug.Log($"[PropertyBlinkSystem] | turning blinking off");
                }
            }
        }
    }

    public struct PreviousPropertyLandedOn : IComponentData
    {
        public Entity Entity;
    }
}
