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
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new CharacterNameIndex { Index = 0 });
            state.EntityManager.CreateSingletonBuffer<ChangeTurnEvent>();
            state.EntityManager.CreateSingleton(new CurrentRound { Value = 0 });

            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GeneralGhostStates>();
            state.RequireForUpdate<PlayersSortedByNetId>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            if (game.AllPlacesInstantiated && game.State != GameState.GameOver &&
                (!SystemAPI.HasComponent<GhostOwner>(activePlayer) ||
                 (SystemAPI.HasComponent<BankruptPlayer>(activePlayer) && SystemAPI.GetComponent<BankruptPlayer>(activePlayer).Value)))
                AdvanceTurn(ref state);
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // TODO: ensure buffer capicty to prevent overloading the server.
            foreach (var (rpc, _, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ChangeTurnRpc>>().WithEntityAccess())
            {
                var active = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var connection = rpc.ValueRO.SourceConnection;
                if (SystemAPI.GetSingleton<GameStateComponent>().State == GameState.GameOver ||
                    !SystemAPI.HasComponent<NetworkId>(connection) || !SystemAPI.HasComponent<GhostOwner>(active) ||
                    SystemAPI.GetComponent<NetworkId>(connection).Value != SystemAPI.GetComponent<GhostOwner>(active).NetworkId)
                {
                    ecb.DestroyEntity(rpcEntity);
                    continue;
                }
                var turnState = SystemAPI.GetSingleton<GameStateComponent>().State;
                if (turnState == GameState.Walking || (turnState == GameState.Rolling &&
                    (!SystemAPI.HasComponent<JailState>(active) || !SystemAPI.GetComponent<JailState>(active).InJail)))
                {
                    ecb.DestroyEntity(rpcEntity);
                    continue;
                }
                // Mandatory property/tax payments must be settled before ending the turn.
                if (SystemAPI.HasComponent<LandingPaymentResolved>(active) &&
                    !SystemAPI.GetComponent<LandingPaymentResolved>(active).Value &&
                    SystemAPI.HasComponent<SpaceLandedOn>(active))
                {
                    var space = SystemAPI.GetComponent<SpaceLandedOn>(active).entity;
                    bool owesTax = SystemAPI.HasComponent<global::DOTS.GameSpaces.TaxSpaceTag>(space);
                    bool owesRent = SystemAPI.HasComponent<OwnerByEntityComponent>(space) &&
                        SystemAPI.GetComponent<OwnerByEntityComponent>(space).Entity != Entity.Null &&
                        SystemAPI.GetComponent<OwnerByEntityComponent>(space).Entity != active;
                    if (owesTax || owesRent) { ecb.DestroyEntity(rpcEntity); continue; }
                }
                AdvanceTurn(ref state);
                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        void AdvanceTurn(ref SystemState state)
        {
            foreach (var jailState in SystemAPI.Query<RefRW<JailState>>().WithAll<ActivePlayer>())
            {
                if (!jailState.ValueRO.InJail || jailState.ValueRO.TurnsInJail == 0) continue;
                jailState.ValueRW.TurnsInJail--;
                if (jailState.ValueRO.TurnsInJail == 0) jailState.ValueRW.InJail = false;
            }
            var totalNumOfCharacters = SystemAPI.GetSingletonBuffer<PlayersSortedByNetId>().Length;
            if (totalNumOfCharacters == 0) return;
            // Publish the next turn in Rolling, never with the previous player's Landing state.
            SystemAPI.GetSingletonRW<GameStateComponent>().ValueRW.State = GameState.Rolling;
            var currentPlayerIndex = SystemAPI.GetSingletonRW<CharacterNameIndex>();
            var nextPlayerIndex = (currentPlayerIndex.ValueRW.Index + 1) % totalNumOfCharacters;
            UnityEngine.Debug.Log($"[ChangeTurnSystem] | nextPlayerIndex: {nextPlayerIndex}");
            UnityEngine.Debug.Log($"[ChangeTurnSystem] | totalNumOfCharacters: {totalNumOfCharacters}");

            var sortedPlayers = SystemAPI.GetSingletonBuffer<PlayersSortedByNetId>();
            for (int attempt = 0; attempt < totalNumOfCharacters; attempt++)
            {
                bool eliminated = true;
                foreach (var (candidateName, bankrupt) in SystemAPI.Query<RefRO<NameComponent>, RefRO<BankruptPlayer>>())
                    if (candidateName.ValueRO.Value == sortedPlayers[nextPlayerIndex].Name) eliminated = bankrupt.ValueRO.Value;
                if (!eliminated) break;
                nextPlayerIndex = (nextPlayerIndex + 1) % totalNumOfCharacters;
            }
            if (nextPlayerIndex <= currentPlayerIndex.ValueRO.Index)
                SystemAPI.GetSingletonRW<CurrentRound>().ValueRW.Value++;
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

        }
    }
}
