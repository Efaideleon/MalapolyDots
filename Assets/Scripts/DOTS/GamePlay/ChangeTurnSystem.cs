using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Assets.Scripts.DOTS.Mediator;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ChangeTurnSystem : ISystem
    {
        private int _currentTurn;
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new CharacterNameIndex { Index = 0 });
            state.EntityManager.CreateSingletonBuffer<ChangeTurnEvent>();
            state.EntityManager.CreateSingleton(new CurrentRound { Value = 0 });

            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GeneralGhostStates>();
            state.RequireForUpdate<PlayersSortedByNetId>();
            _currentTurn = 0;
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // TODO: ensure buffer capicty to prevent overloading the server.
            foreach (var (rpc, _, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ChangeTurnRpc>>().WithEntityAccess())
            {
                // Why is this running every frame after clicking Change Turn? isn't the entity being destroyed after the event is processed???
                // Handle each change turn request
                //var totalRounds = SystemAPI.GetSingleton<LoginData>().NumberOfRounds;
                //var totalNumOfPlayer = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;

                var totalNumOfCharacters = SystemAPI.GetSingleton<GeneralGhostStates>().TotalNumberOfCharSpawned;
                _currentTurn += 1;
                UnityEngine.Debug.Log($"Turn: {_currentTurn}");
                if (_currentTurn == totalNumOfCharacters)
                {
                    SystemAPI.GetSingletonRW<CurrentRound>().ValueRW.Value += 1;
                    _currentTurn = 0;
                    UnityEngine.Debug.Log($"[ChangeTurnSystem] | Changing Round {SystemAPI.GetSingleton<CurrentRound>().Value}");
                }

                var currentPlayerIndex = SystemAPI.GetSingletonRW<CharacterNameIndex>();
                var nextPlayerIndex = (currentPlayerIndex.ValueRW.Index + 1) % totalNumOfCharacters;
                UnityEngine.Debug.Log($"[ChangeTurnSystem] | nextPlayerIndex: {nextPlayerIndex}");
                UnityEngine.Debug.Log($"[ChangeTurnSystem] | totalNumOfCharacters: {totalNumOfCharacters}");

                currentPlayerIndex.ValueRW.Index = nextPlayerIndex;

                foreach (var (name, playerID, activePlayer, entity) in
                        SystemAPI.Query<
                        RefRO<NameComponent>,
                        RefRO<PlayerID>,
                        EnabledRefRW<ActivePlayer>
                        >()
                        .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
                        .WithEntityAccess())
                {
                    var characterSelectedNames = SystemAPI.GetSingletonBuffer<PlayersSortedByNetId>();
                    if (characterSelectedNames[currentPlayerIndex.ValueRO.Index].Name == name.ValueRO.Value)
                    {
                        SystemAPI.GetSingletonRW<CurrentPlayerID>().ValueRW.Value = playerID.ValueRO.Value;
                        SystemAPI.GetSingletonBuffer<ChangeTurnEvent>().Add(new ChangeTurnEvent { });
                        SystemAPI.GetSingletonRW<CurrentPlayerComponent>().ValueRW.entity = entity;

                        SystemAPI.GetSingletonRW<CurrentActivePlayer>().ValueRW.Entity = entity;
                        activePlayer.ValueRW = true;
                    }
                    else
                    {
                        activePlayer.ValueRW = false;
                    }
                }

                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
