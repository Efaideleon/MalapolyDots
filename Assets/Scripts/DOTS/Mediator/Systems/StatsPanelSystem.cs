using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public partial struct StatsPanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<CurrentPlayerID>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (playerID, money) in
                    SystemAPI.Query<RefRO<PlayerID>, RefRO<MoneyComponent>>().WithChangeFilter<MoneyComponent>())
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    if (panelControllers.statsPanelController != null)
                    {
                        var currPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                        if (playerID.ValueRO.Value == currPlayerID.Value)
                        {
                            var currContext = panelControllers.statsPanelController.Context;
                            StatsPanelContext newContext = new()
                            {
                                Name = currContext.Name,
                                Money = money.ValueRO.Value.ToString()
                            };
                            panelControllers.statsPanelController.Context = newContext; 
                            panelControllers.statsPanelController.Update();
                        }
                    }
                }
            }
        }
    }
}
