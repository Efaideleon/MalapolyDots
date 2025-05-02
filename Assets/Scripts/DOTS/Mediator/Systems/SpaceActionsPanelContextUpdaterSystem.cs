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
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            bool shouldUpdate = false;
            foreach ( var _ in SystemAPI.Query<RefRO<MonopolyFlagComponent>>().WithChangeFilter<MonopolyFlagComponent>())
            {
                shouldUpdate = true;
            }

            foreach ( var _ in SystemAPI.Query<RefRO<LastPropertyClicked>>().WithChangeFilter<LastPropertyClicked>())
            {
                shouldUpdate = true;
            }

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

                    SystemAPI.SetSingleton(new SpaceActionsPanelContextComponent { Value = spaceActionsContext });
                }
            }

            foreach (var onLandEntity in SystemAPI.Query<RefRO<LandedOnSpace>>().WithChangeFilter<LandedOnSpace>())
            {
                if (onLandEntity.ValueRO.entity != Entity.Null)
                {
                    if (SystemAPI.HasComponent<PropertySpaceTag>(onLandEntity.ValueRO.entity))
                    {
                        var hasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(onLandEntity.ValueRO.entity);
                        var owner = SystemAPI.GetComponent<OwnerComponent>(onLandEntity.ValueRO.entity);
                        var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                        bool isCurrentOwner = currentPlayerID.Value == owner.ID;
                        SpaceActionsPanelContext spaceActionsContext = new()
                        {
                            HasMonopoly = hasMonopoly.Value,
                            IsPlayerOwner = isCurrentOwner
                        };

                        SystemAPI.SetSingleton(new SpaceActionsPanelContextComponent { Value = spaceActionsContext });
                    }
                }
            }
        }
    }
}
