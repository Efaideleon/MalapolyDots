using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using Assets.Common;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct StartHostingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<HostSetupHostClickEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            //Debug.Log($"ServerWorld exists? {ClientServerBootstrap.ServerWorld != null}");

            // TODO: make sure to know that the relay and sessions are ready.
            // TODO: only enable the start button once the lobby has been created.
            if (ClientServerBootstrap.ServerWorld == null)
                return;

            if (!NetworkRequests.StartHost)
                return;

            NetworkRequests.StartHost = false;

            var serverWorld = ClientServerBootstrap.ServerWorld;
            var serverEM = serverWorld.EntityManager;
            if (!serverEM.CreateEntityQuery(typeof(NetworkStreamConnection)).IsEmpty)
                return;

            // Server setup
            //ushort port = 7979;
            var listenEntity = serverEM.CreateEntity();
            //serverEM.AddComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4.WithPort(port) });
            serverEM.AddComponentData(listenEntity, new NetworkStreamRequestListen { });

            // Local Client Setup
            var connectEntity = state.EntityManager.CreateEntity();
            //state.EntityManager.AddComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port) });
            state.EntityManager.AddComponentData(connectEntity, new NetworkStreamRequestConnect { });

            //var clickEntity = SystemAPI.GetSingletonEntity<HostSetupHostClickEvent>();
            //state.EntityManager.DestroyEntity(clickEntity);
            Debug.Log("Starting Host Server");
        }
    }
}

