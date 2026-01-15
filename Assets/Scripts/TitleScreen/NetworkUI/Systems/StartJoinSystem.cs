using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct StartJoinSystem : ISystem
    {
        private EntityQuery networkStreamQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JoinSetupJoinClickEvent>();
            networkStreamQuery = state.EntityManager.CreateEntityQuery(typeof(NetworkStreamConnection));
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!networkStreamQuery.IsEmpty)
                return;

            ushort port = 7979;
            string ip = "127.0.0.1";

            // Client Setup
            var connectEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(connectEntity, new NetworkStreamRequestConnect { Endpoint = NetworkEndpoint.Parse(ip, port) });

            // TODO: Handle if there is no host, or connection can't be established.
            Debug.Log("Joining Server");
        }
    }
}
