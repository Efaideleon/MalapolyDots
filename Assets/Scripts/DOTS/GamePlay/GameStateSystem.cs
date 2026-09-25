using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public enum GameState
    {
        Rolling,
        Walking,
        Landing,
        GameOver,
    }

    public struct TurnChangedFlag : IComponentData
    {
        public bool Flag;
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(SpaceDetectorSystem))]
    public partial struct GamePlaySystem : ISystem
    {
        public ComponentLookup<FinalArrived> finalArrivedLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<CharactersSpawnedTag>();
            state.RequireForUpdate<GameStateComponent>();

            finalArrivedLookup = SystemAPI.GetComponentLookup<FinalArrived>();

            state.RequireForUpdate<CurrentActivePlayer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.GetSingleton<GameStateComponent>().State == GameState.GameOver) return;
            finalArrivedLookup.Update(ref state);
            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayerEntity == null)
            {
                return;
            }

            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                {
                    gameState.ValueRW.State = GameState.Rolling;
                }
            }

            foreach (var rollComponent in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
            {
                UnityEngine.Debug.Log($"[GamePlaySystem] | RollAmountChanged");
                if (rollComponent.ValueRO.Value > 0)
                {
                    foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                    {
                        UnityEngine.Debug.Log($"[GamePlaySystem] | Setting gamestate to walking");
                        gameState.ValueRW.State = GameState.Walking;
                    }
                }
            }

            if (!finalArrivedLookup.HasComponent(activePlayerEntity))
                return;

            if (finalArrivedLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                var arrived = finalArrivedLookup.GetRefRW(activePlayerEntity);
                if (arrived.ValueRO.Value == true)
                {
                    var game = SystemAPI.GetSingletonRW<GameStateComponent>();
                    game.ValueRW.State = GameState.Landing;
                    game.ValueRW.LandingPlayer = activePlayerEntity;
                    game.ValueRW.LandingSpace = SystemAPI.GetComponent<SpaceLandedOn>(activePlayerEntity).entity;
                    game.ValueRW.LandingSequence++;
                    if (game.ValueRO.LandingSequence == 0) game.ValueRW.LandingSequence = 1;
                    arrived.ValueRW.Value = false;
                }
            }
        }
    }
}

namespace DOTS.GamePlay
{
    // This game has no mortgages or liquidation: inability to pay cash ends a player's game.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(PayRentSystem))]
    [UpdateAfter(typeof(Assets.Scripts.DOTS.GamePlay.PayTaxesSystem))]
    [UpdateAfter(typeof(TreasureSystem))]
    [UpdateAfter(typeof(PickRandomChanceCardSystem))]
    [UpdateBefore(typeof(Assets.Scripts.DOTS.GamePlay.ChangeTurnSystem))]
    public partial struct BankruptcySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<BankruptPlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            if (!game.AllPlacesInstantiated || game.State == GameState.GameOver) return;
            int players = 0, survivors = 0, winner = 0;
            foreach (var (money, bankrupt, owner, player) in SystemAPI.Query<RefRO<GhostMoneyComponet>, RefRW<BankruptPlayer>, RefRO<GhostOwner>>().WithEntityAccess())
            {
                players++;
                bool connected = false;
                foreach (var connection in SystemAPI.Query<RefRO<NetworkId>>())
                    if (connection.ValueRO.Value == owner.ValueRO.NetworkId) { connected = true; break; }
                if ((money.ValueRO.Value < 0 || !connected) && !bankrupt.ValueRO.Value)
                {
                    bankrupt.ValueRW.Value = true;
                    // Return eliminated players' properties to the bank.
                    foreach (var (propertyOwner, ownerEntity, property) in SystemAPI.Query<RefRW<DOTS.DataComponents.OwnerComponent>, RefRW<DOTS.DataComponents.OwnerByEntityComponent>>().WithEntityAccess())
                    {
                        if (ownerEntity.ValueRO.Entity != player) continue;
                        propertyOwner.ValueRW.ID = DOTS.Constants.PropertyConstants.Vacant;
                        ownerEntity.ValueRW.Entity = Entity.Null;
                        if (SystemAPI.HasComponent<DOTS.DataComponents.MonopolyFlagComponent>(property))
                            SystemAPI.SetComponent(property, new DOTS.DataComponents.MonopolyFlagComponent());
                        if (SystemAPI.HasComponent<DOTS.DataComponents.HouseCount>(property))
                            SystemAPI.SetComponent(property, new DOTS.DataComponents.HouseCount());
                        if (SystemAPI.HasComponent<DOTS.DataComponents.GhostRentComponent>(property))
                            SystemAPI.SetComponent(property, new DOTS.DataComponents.GhostRentComponent());
                    }
                }
                if (!bankrupt.ValueRO.Value) { survivors++; winner = owner.ValueRO.NetworkId; }
            }
            if ((players >= 2 && survivors <= 1) || (players > 0 && survivors == 0))
            {
                game.WinnerNetworkId = survivors == 1 ? winner : 0;
                game.State = GameState.GameOver;
                SystemAPI.SetSingleton(game);
            }
        }
    }
}
