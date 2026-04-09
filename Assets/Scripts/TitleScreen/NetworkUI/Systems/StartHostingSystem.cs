using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct StartHostingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<HostSetupHostClickEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Debug.Log($"ServerWorld exists? {ClientServerBootstrap.ServerWorld != null}");

            if (ClientServerBootstrap.ServerWorld == null)
                return;

            var serverWorld = ClientServerBootstrap.ServerWorld;
            var serverEM = serverWorld.EntityManager;
            if (!serverEM.CreateEntityQuery(typeof(NetworkStreamConnection)).IsEmpty)
                return;

            // Server setup
            ushort port = 7979;
            var listenEntity = serverEM.CreateEntity();
            serverEM.AddComponentData(listenEntity, new NetworkStreamRequestListen { Endpoint = NetworkEndpoint.AnyIpv4.WithPort(port) });

            // Local Client Setup
            var connectEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port) });

            var clickEntity = SystemAPI.GetSingletonEntity<HostSetupHostClickEvent>();
            state.EntityManager.DestroyEntity(clickEntity);
            Debug.Log("Starting Host Server");
        }
    }
}

