using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Systems
{
    public struct SpaceActionsPanelContextComponent : IComponentData
    {
        public SpaceActionsPanelContext Value;
    }

    [BurstCompile]
    public partial struct SpaceActionsPanelContextUpdaterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new SpaceActionsPanelContextComponent { Value = default });
            state.RequireForUpdate<SpaceActionsPanelContextComponent>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<PropertyEventComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            bool shouldUpdate = false;
            foreach ( var _ in SystemAPI.Query<RefRO<MonopolyFlagComponent>>().WithChangeFilter<MonopolyFlagComponent>())
                shouldUpdate = true;

            if (shouldUpdate)
            {
                var clickedPropertyEntity = SystemAPI.GetSingleton<LastPropertyClicked>().entity;
                if (clickedPropertyEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(clickedPropertyEntity))
                {
                    var hasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(clickedPropertyEntity);
                    var owner = SystemAPI.GetComponent<OwnerComponent>(clickedPropertyEntity);
                    var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                    bool isCurrentOwner = currentPlayerID.Value == owner.ID;
                    SpaceActionsPanelContext spaceActionsContext = new()
                    {
                        HasMonopoly = hasMonopoly.Value,
                        IsPlayerOwner = isCurrentOwner
                    };

                    var panelContext = SystemAPI.GetSingletonRW<SpaceActionsPanelContextComponent>();
                    panelContext.ValueRW = new SpaceActionsPanelContextComponent { Value = spaceActionsContext };
                }
            }

            foreach (var property in SystemAPI.Query<RefRO<PropertyEventComponent>>().WithChangeFilter<PropertyEventComponent>())
            {
                var propertyEntity = property.ValueRO.entity;
                if (propertyEntity != Entity.Null)
                {
                    var hasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(propertyEntity);
                    var owner = SystemAPI.GetComponent<OwnerComponent>(propertyEntity);
                    var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                    bool isCurrentOwner = currentPlayerID.Value == owner.ID;
                    SpaceActionsPanelContext spaceActionsContext = new()
                    {
                        HasMonopoly = hasMonopoly.Value,
                        IsPlayerOwner = isCurrentOwner
                    };

                    var panelContext = SystemAPI.GetSingletonRW<SpaceActionsPanelContextComponent>();
                    panelContext.ValueRW = new SpaceActionsPanelContextComponent { Value = spaceActionsContext };
                }
            }
        }
    }
}
