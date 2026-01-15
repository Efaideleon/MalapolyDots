using Assets.Scripts.DOTS.DataComponents;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RequestCharacterSelectedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // Avocado
            foreach (var req in SystemAPI.Query<RefRO<AvocadoClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Avocado });
            }

            // Bird
            foreach (var req in SystemAPI.Query<RefRO<BirdClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Bird });
            }

            // Coin
            foreach (var req in SystemAPI.Query<RefRO<CoinClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Coin });
            }

            // Lira
            foreach (var req in SystemAPI.Query<RefRO<LiraClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Lira });
            }

            // Coffee
            foreach (var req in SystemAPI.Query<RefRO<CoffeeClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Coffee });
            }

            // Tuctuc
            foreach (var req in SystemAPI.Query<RefRO<TuctucClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent(rpcEntity, new PrepickCharacter { Character = CharactersEnum.Tuctuc });
            }

            foreach (var req in SystemAPI.Query<RefRO<CharacterSelectConfirmClickEvent>>())
            {
                var rpcEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                ecb.AddComponent<LockInCharacterEvent>(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct LockInCharacterEvent : IRpcCommand
    {}

    public struct PrepickCharacter : IRpcCommand
    {
        public CharactersEnum Character;
    }
}
