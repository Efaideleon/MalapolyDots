using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GoToMainMenuSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoToMainMenuRpc>>().WithEntityAccess())
            {
                var gameMenuUIPhase = SystemAPI.GetSingletonRW<GameMenuPhaseComponent>();
                gameMenuUIPhase.ValueRW.Value = GameMenuPhase.MainMenu;
                UnityEngine.Debug.Log($"[GoToMainMenuSystem] | Going to main menu {state.World}");
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct GoToMainMenuRpc : IRpcCommand
    {}
}