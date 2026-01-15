using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GoToCharacterSelectClientSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var evt in SystemAPI.Query<RefRO<LobbyStartClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent(rpcEntity, new GoToCharacterSelectRpc { });
                ecb.AddComponent(rpcEntity, new SendRpcCommandRequest { });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct AllGoToCharacterSelectClientSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        { }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (receivedRequest, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoToCharacterSelectRpc>().WithEntityAccess())
            {
                // TODO: Make sure that the client is able to move to the character select
                if(SystemAPI.TryGetSingletonRW<GameMenuPhaseComponent>(out var gamePhase))
                {
                    if (gamePhase.ValueRO.Value == GameMenuPhase.Lobby)
                    {
                        UnityEngine.Debug.Log($"[AllGoToCharacterSelectClientSystem] | we are in the lobby, change ui to character select");
                        gamePhase.ValueRW.Value = GameMenuPhase.CharacterSelect;
                    }
                }
                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GoToCharacterSelectServerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (receivedRequest, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoToCharacterSelectRpc>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"[GoToCharacterSelectServerSystem] | Server received change to character select screen");

                var goToCharacterSelectRpcEntity = ecb.CreateEntity();
                ecb.AddComponent<GoToCharacterSelectRpc>(goToCharacterSelectRpcEntity);
                ecb.AddComponent(goToCharacterSelectRpcEntity, new SendRpcCommandRequest { TargetConnection = Entity.Null });

                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct GoToCharacterSelectRpc : IRpcCommand
    { }
}
