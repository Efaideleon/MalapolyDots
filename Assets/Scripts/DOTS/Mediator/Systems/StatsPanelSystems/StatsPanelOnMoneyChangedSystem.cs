using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator.Systems;
using Assets.Scripts.DOTS.UI.Controllers;
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
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<GhostMoneyComponet>();
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<StatsPanelRegistrationCompleteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (name, money, owner) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostMoneyComponet>, RefRO<GhostOwner>>()

                    .WithAll<CharacterFlag>())
            {
                var panelService = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                if (panelService.TryGet<StatsPanelController>(out var statsPanel))
                {
                    StatsPanelContext statsPanelContext = new()
                    {
                        Name = name.ValueRO.Value,
                        Money = money.ValueRO.Value < 0 ? "Bankrupt" :
                            (SystemAPI.TryGetSingleton<Assets.Scripts.DOTS.GamePlay.GameStateComponent>(out var game) &&
                            game.State == DOTS.GamePlay.GameState.GameOver && game.WinnerNetworkId == owner.ValueRO.NetworkId ? "Winner · " : "") + money.ValueRO.Value.ToString("N0")
                    };
                    statsPanel.LoadPanelData(statsPanelContext);
                }
            }
        }
    }
}
