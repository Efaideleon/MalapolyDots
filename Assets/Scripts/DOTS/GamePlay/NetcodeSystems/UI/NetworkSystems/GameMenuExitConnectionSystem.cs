using TitleScreen.NetworkUI.Components;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GameMenuExitConnectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var evt in SystemAPI.Query<RefRO<ExitConnectionClickEvent>>())
            {
                UnityEngine.Debug.Log($"[GameMenuBackButtonSystem] | ExitConnection pressed.");
                var rpcEntity = ecb.CreateEntity();
                // ecb.AddComponent(rpcEntity, new GoToCharacterSelectRpc { });
                // ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
