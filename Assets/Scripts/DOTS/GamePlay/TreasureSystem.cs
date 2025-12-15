using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay.PropertyAnimations;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct TreasureCard : IComponentData
    {
        public FixedString64Bytes data;
    }

    [BurstCompile]
    public partial struct TreasureSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new TreasureCard { data = default });
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<TreasureAnimationBuffer>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<SpaceLandedOn>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();
                    var landedOnEntity = SystemAPI.GetComponent<SpaceLandedOn>(activePlayer.Entity).entity;
                    if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity))
                    {
                        var cards = SystemAPI.GetBuffer<TreasureCardsBuffer>(landedOnEntity);
                        var random = SystemAPI.GetSingletonRW<RandomValueComponent>();
                        var randomIdx = random.ValueRW.Value.NextInt(0, cards.Length);
                        var cardChosen = cards[randomIdx];
                        var treasureCard = SystemAPI.GetSingletonRW<TreasureCard>();
                        treasureCard.ValueRW.data = cardChosen.msg;

//                        UnityEngine.Debug.Log($"[TreasureSystem] | cardChosen msg: {treasureCard.ValueRO.data.ToString()}");

                        // Reset that treasure's open animation.
                        SystemAPI.GetComponentRW<CurrentTreasureAnimation>(landedOnEntity).ValueRW.Value = TreasureAnimations.Open;
                        SystemAPI.GetComponentRW<AnimationPlayState>(landedOnEntity).ValueRW.Value = PlayState.Playing;
                        UnityEngine.Debug.Log($"[TreasureSystem] | Set Animation to open");
                    }
                }
            }
        }
    }
}
