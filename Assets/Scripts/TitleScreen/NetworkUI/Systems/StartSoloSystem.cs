using System;
using Assets.Common;
using DOTS.DataComponents;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class StartSoloSystem : SystemBase
    {
        bool connecting, requestedSelection;
        double deadline;

        protected override void OnCreate() => RequireForUpdate<GameMenuPhaseComponent>();

        protected override void OnUpdate()
        {
            if (SoloSession.StartRequested)
            {
                SoloSession.StartRequested = false;
                try
                {
                    using var connections = EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection));
                    if (!connections.IsEmptyIgnoreFilter) throw new InvalidOperationException("Leave the current table before starting a solo game.");
                    var server = ClientServerBootstrap.ServerWorld ?? ClientServerBootstrap.CreateServerWorld("Solo Server");
                    ConfigureLocalDriver(server, true);
                    ConfigureLocalDriver(World, false);
                    using var drivers = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                    ref var driver = ref drivers.GetSingletonRW<NetworkStreamDriver>().ValueRW;
                    if (!driver.DriverStore.HasListeningInterfaces && !driver.Listen(NetworkEndpoint.LoopbackIpv4.WithPort(0)))
                        throw new InvalidOperationException("The local game could not start. Please try again.");
                    var endpoint = driver.GetLocalEndPoint();
                    using var clientDrivers = EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                    clientDrivers.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(EntityManager, endpoint);
                    SoloSession.Active = true;
                    SoloSession.Paused = false;
                    SoloSession.Opponents = Math.Clamp(SoloSession.Opponents, 1, 5);
                    NetworkRequests.LobbyVersion++;
                    NetworkRequests.ExpectedLobbyPlayers = 1;
                    NetworkRequests.MatchRoundLimit = NetworkRequests.SelectedRoundLimit;
                    NetworkRequests.StartHost = NetworkRequests.StartClient = NetworkRequests.StartGame = false;
                    connecting = true;
                    requestedSelection = false;
                    deadline = UnityEngine.Time.realtimeSinceStartupAsDouble + 30;
                }
                catch (Exception error)
                {
                    SoloSession.Reset();
                    SoloSession.Status = error.Message;
                }
            }
            if (!connecting) return;
            if (SystemAPI.GetSingleton<GameMenuPhaseComponent>().Value == GameMenuPhase.CharacterSelect)
            {
                connecting = false;
                SoloSession.Status = "";
                return;
            }
            if (!requestedSelection && SystemAPI.HasSingleton<NetworkId>())
            {
                NetworkRequests.StartGame = true;
                SystemAPI.SetSingleton(new NetworkRoleTypeComponent { Value = NetworkRole.Host });
                requestedSelection = true;
            }
            // Stay on solo setup while the local connection completes.
            if (SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var panels))
            {
                foreach (var panel in panels.AllPanels) panel.Hide();
                panels.PanelLookup[GameMenuPhase.SoloSetup].Show();
            }
            if (UnityEngine.Time.realtimeSinceStartupAsDouble < deadline) return;
            connecting = false;
            SoloSession.Status = "The solo table could not connect. Return to the main menu and try again.";
            NetworkRequests.ReturnToMainMenu?.Invoke();
        }

        static void ConfigureLocalDriver(World world, bool server)
        {
            world.EntityManager.CompleteAllTrackedJobs();
            var settings = server ? DefaultDriverBuilder.GetNetworkServerSettings() : DefaultDriverBuilder.GetNetworkClientSettings();
            try
            {
                // An in-process solo connection must survive a pause of any length.
                settings.WithNetworkConfigParameters(disconnectTimeoutMS: 0, reconnectionTimeoutMS: 0);
                var store = new NetworkDriverStore();
                var instance = server ? DefaultDriverBuilder.CreateServerNetworkDriver(new IPCNetworkInterface(), settings) :
                    DefaultDriverBuilder.CreateClientNetworkDriver(new IPCNetworkInterface(), settings);
                store.RegisterDriver(TransportType.IPC, instance);
                using var query = world.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
                query.GetSingletonRW<NetworkStreamDriver>().ValueRW.ResetDriverStore(world.Unmanaged, ref store);
            }
            finally { settings.Dispose(); }
        }
    }
}
