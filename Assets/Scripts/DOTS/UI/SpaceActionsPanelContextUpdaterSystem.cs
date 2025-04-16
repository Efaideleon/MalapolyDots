using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Burst;
using Unity.Entities;

public struct SpaceActionsPanelContextComponent : IComponentData
{
    public SpaceActionsPanelContext Value;
}

public struct PurchasePropertyPanelContextComponent : IComponentData
{
    public PurchasePropertyPanelContext Value;
}

[BurstCompile]
public partial struct SpaceActionsPanelContextUpdaterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new SpaceActionsPanelContextComponent { Value = default });
        state.EntityManager.CreateSingleton(new PurchasePropertyPanelContextComponent { Value = default });
        state.RequireForUpdate<SpaceActionsPanelContextComponent>();
        state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
        state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
        state.RequireForUpdate<LastPropertyClicked>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ( var _ in SystemAPI.Query<RefRO<MonopolyFlagComponent>>().WithChangeFilter<MonopolyFlagComponent>())
        {
            var lastPropertyClicked =  SystemAPI.GetSingleton<LastPropertyClicked>();
            if (lastPropertyClicked.entity != Entity.Null)
            {
                var hasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(lastPropertyClicked.entity);
                var owner = SystemAPI.GetComponent<OwnerComponent>(lastPropertyClicked.entity);
                var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                bool isCurrentOwner = currentPlayerID.Value == owner.ID;
                SpaceActionsPanelContext spaceActionsContext = new()
                {
                    HasMonopoly = hasMonopoly.Value,
                    IsPlayerOwner = isCurrentOwner
                };

                SystemAPI.SetSingleton(new SpaceActionsPanelContextComponent { Value = spaceActionsContext });
                continue;
            }
        }

        foreach ( var clickedProperty in 
                SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                >().
                WithChangeFilter<LastPropertyClicked>())
        {
            if (clickedProperty.ValueRO.entity != Entity.Null)
            {
                PurchaseHousePanelContext purchaseHouseContext = new()
                {
                    Name = SystemAPI.GetComponent<NameComponent>(clickedProperty.ValueRO.entity).Value,
                    HousesOwned = SystemAPI.GetComponent<HouseCount>(clickedProperty.ValueRO.entity).Value,
                    Price = 10,
                };
                SystemAPI.SetSingleton(new PurhcaseHousePanelContextComponent { Value = purchaseHouseContext});

                PurchasePropertyPanelContext purchasePropertyPanelContext = new()
                {
                    spaceEntity = clickedProperty.ValueRO.entity,
                    entityManager = state.EntityManager,
                    playerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                };
                SystemAPI.SetSingleton(new PurchasePropertyPanelContextComponent { Value = purchasePropertyPanelContext });
            }
        }
    }
}
