using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    public partial struct StatsPanelOnMoneyChangedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (playerID, name, money) in
                    SystemAPI.Query<
                    RefRO<PlayerID>,
                    RefRO<NameComponent>,
                    RefRO<MoneyComponent>
                    >()
                    .WithChangeFilter<MoneyComponent>())
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    if (panelControllers.statsPanelController != null)
                    {
                        var currPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                        if (playerID.ValueRO.Value == currPlayerID.Value)
                        {
                            StatsPanelContext statsPanelContext = new() 
                            { 
                                Name = name.ValueRO.Value.ToString(),
                                Money = money.ValueRO.Value.ToString()
                            };
                            UnityEngine.Debug.Log($"new money: {statsPanelContext.Money}");
                            panelControllers.statsPanelController.Context = statsPanelContext;
                            panelControllers.statsPanelController.Update();
                        }
                    }
                }
            }
        }
    }
}
