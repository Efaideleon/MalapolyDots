using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator.Systems;
using DOTS.DataComponents;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct StatsPanelOnMoneyChangedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<StatsPanelRegistrationCompleteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (name, money) in
                    SystemAPI.Query<
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
                        StatsPanelContext statsPanelContext = new() 
                        { 
                            Name = name.ValueRO.Value.ToString(),
                            Money = money.ValueRO.Value.ToString()
                        };
                        panelControllers.statsPanelController.LoadPanelData(statsPanelContext);
                    }
                }
            }
        }
    }
}
