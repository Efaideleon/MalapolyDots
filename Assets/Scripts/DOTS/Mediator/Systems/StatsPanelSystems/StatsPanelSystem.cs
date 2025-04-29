using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    public partial struct StatsPanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<CurrentPlayerID>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var currPlayerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                foreach (var (playerID, name, money) in
                        SystemAPI.Query<
                        RefRO<PlayerID>,
                        RefRO<NameComponent>,
                        RefRO<MoneyComponent>
                        >())
                {
                    PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers != null)
                    {
                        if (panelControllers.statsPanelController != null)
                        {
                            if (playerID.ValueRO.Value == currPlayerID.ValueRO.Value)
                            {
                                StatsPanelContext statsPanelContext = new() 
                                { 
                                    Name = name.ValueRO.Value.ToString(),
                                    Money = money.ValueRO.Value.ToString()
                                };
                                panelControllers.statsPanelController.Context = statsPanelContext;
                                panelControllers.statsPanelController.Update();
                            }
                        }
                    }
                }
            }
        }
    }
}
