using DOTS.Characters;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct LastPropertyClicked : IComponentData
    {
        public Entity entity;
    }

    [BurstCompile]
    public partial struct PropertyEventSystem : ISystem
    {
        private ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        private ComponentLookup<PropertySpaceTag> propertySpaceLookup;
        private ComponentLookup<LastPropertyClicked> lastPropertyClickedLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new LastPropertyInteracted { entity = Entity.Null });
            state.EntityManager.CreateSingleton(new LastPropertyClicked { entity = Entity.Null });

            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<CurrentActivePlayer>();

            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>(true);
            propertySpaceLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>(true);
            lastPropertyClickedLookup = SystemAPI.GetComponentLookup<LastPropertyClicked>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            spaceLandedOnLookup.Update(ref state);
            propertySpaceLookup.Update(ref state);
            lastPropertyClickedLookup.Update(ref state);

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var lpcRefEntity = SystemAPI.GetSingletonEntity<LastPropertyClicked>();
            var lastSystemVersion = state.LastSystemVersion;

            // Recording the property entity we last clicked.
            if (lastPropertyClickedLookup.DidChange(lpcRefEntity, lastSystemVersion))
            {
                var lastPropertyClickedEntity = lastPropertyClickedLookup[lpcRefEntity].entity;
                if (propertySpaceLookup.HasComponent(lastPropertyClickedEntity))
                {
                    SystemAPI.GetSingletonRW<LastPropertyInteracted>().ValueRW.entity = lastPropertyClickedEntity;
                }
            }

            // Recording the property entity we landed on.
            if (spaceLandedOnLookup.DidChange(activePlayerEntity, lastSystemVersion))
            {
                var onLandSpaceEntity = spaceLandedOnLookup[activePlayerEntity].entity;
                if (propertySpaceLookup.HasComponent(onLandSpaceEntity))
                {
                    SystemAPI.GetSingletonRW<LastPropertyInteracted>().ValueRW.entity = onLandSpaceEntity;
                }
            }
        }
    }
}
