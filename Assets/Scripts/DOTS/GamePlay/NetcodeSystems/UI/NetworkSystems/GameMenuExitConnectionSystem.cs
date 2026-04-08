using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GameMenuExitConnectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var connection = SystemAPI.GetSingletonEntity<NetworkStreamConnection>();
            foreach (var evt in SystemAPI.Query<RefRO<ExitConnectionClickEvent>>())
            {
                UnityEngine.Debug.Log($"[GameMenuExitConnectionSystem] | ExitConnection pressed.");
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent(connection, new NetworkStreamRequestDisconnect { Reason = NetworkStreamDisconnectReason.ConnectionClose });

                var gameMenuUIPhase = SystemAPI.GetSingletonRW<GameMenuPhaseComponent>();
                gameMenuUIPhase.ValueRW.Value = GameMenuPhase.MainMenu;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
