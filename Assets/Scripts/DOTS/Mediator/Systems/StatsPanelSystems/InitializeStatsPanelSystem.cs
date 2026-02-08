using System.Collections.Generic;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using DOTS.Mediator;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.Mediator.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializeStatsPanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GeneralGhostStates>();
            state.RequireForUpdate<PlayersSortedByNetId>();
            state.RequireForUpdate<UIPanelResolved>();
            state.RequireForUpdate<GhostDataLoadedTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var panelsResolved = SystemAPI.GetSingleton<UIPanelResolved>();
            if (!panelsResolved.IsStatsPanelResolved)
                return;

            var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers.statsPanelController == null)
                return;

            var generalStates = SystemAPI.GetSingleton<GeneralGhostStates>();
            if (!generalStates.AllGhostCharactersSpawned || !generalStates.PlayerNamesSorted)
                return;

            var playerNamesBuffer = SystemAPI.GetSingletonBuffer<PlayersSortedByNetId>();

            List<string> orderedNames = new();
            foreach (var player in playerNamesBuffer)
            {
                orderedNames.Add(player.Name.ToString());
            }

            panelControllers.statsPanelController.SetupPanels(orderedNames);

            state.EntityManager.CreateSingleton<StatsPanelRegistrationCompleteTag>();
            state.Enabled = false;
        }
    }

    public struct PlayerPanelsInstantiatedTag : IComponentData
    { }

    public struct StatsPanelRegistrationCompleteTag : IComponentData
    { }
}
