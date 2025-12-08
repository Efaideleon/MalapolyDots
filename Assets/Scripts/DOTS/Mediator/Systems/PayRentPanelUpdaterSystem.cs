using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Systems
{
    public struct PayRentPanelContextComponent : IComponentData
    {
        public PayRentPanelContext Value;
    }

    [BurstCompile]
    public partial struct PayRentPanelUpdaterSystem : ISystem
    {
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<LastPropertyClicked> lastPropertyClickedLookup;
        public ComponentLookup<PropertySpaceTag> propertySpaceLookup;
        public ComponentLookup<RentComponent> rentLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new PayRentPanelContextComponent { Value = default });
            state.RequireForUpdate<PayRentPanelContextComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<CurrentActivePlayer>();

            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>(true);
            lastPropertyClickedLookup = SystemAPI.GetComponentLookup<LastPropertyClicked>(true);
            propertySpaceLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>(true);
            rentLookup = SystemAPI.GetComponentLookup<RentComponent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            lastPropertyClickedLookup.Update(ref state);
            spaceLandedOnLookup.Update(ref state);
            propertySpaceLookup.Update(ref state);
            rentLookup.Update(ref state);

            var lastPropertyClickedComponentEntity = SystemAPI.GetSingletonEntity<LastPropertyClicked>();
            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (lastPropertyClickedLookup.HasComponent(lastPropertyClickedComponentEntity)
                    && lastPropertyClickedLookup.DidChange(lastPropertyClickedComponentEntity, state.LastSystemVersion))
            {
                var propertyClickedEntity = lastPropertyClickedLookup[lastPropertyClickedComponentEntity].entity;
                if (propertyClickedEntity != Entity.Null)
                {
                    if (rentLookup.HasComponent(propertyClickedEntity))
                    {
                        UpdatePayRentPanel(ref state, rentLookup[propertyClickedEntity].Value);
                    }
                }
            }

            if (spaceLandedOnLookup.HasComponent(activePlayerEntity) && spaceLandedOnLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                var spaceLandedOnEntity = spaceLandedOnLookup[activePlayerEntity].entity;
                if (spaceLandedOnEntity != Entity.Null && propertySpaceLookup.HasComponent(spaceLandedOnEntity))
                {
                    if (rentLookup.HasComponent(spaceLandedOnEntity))
                    {
                        UpdatePayRentPanel(ref state, rentLookup[spaceLandedOnEntity].Value);
                    }
                }
            }
        }

        [BurstCompile]
        public void UpdatePayRentPanel(ref SystemState _, int rent)
        {
            PayRentPanelContext payRentPanelContext = new()
            {
                Rent = rent
            };
            var panelContext = SystemAPI.GetSingletonRW<PayRentPanelContextComponent>();
            panelContext.ValueRW = new PayRentPanelContextComponent { Value = payRentPanelContext };
        }
    }
}
