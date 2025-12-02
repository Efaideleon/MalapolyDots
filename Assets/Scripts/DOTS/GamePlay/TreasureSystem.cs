using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay.PropertyAnimations;
using DOTS.GameSpaces;
using DOTS.Utilities.TreasuresBlob;
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
            state.RequireForUpdate<TreasuresDataBlobReference>();
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<TreasureAnimationBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>().entity;
                    if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity))
                    {
                        var treasuresBlobReference = SystemAPI.GetSingleton<TreasuresDataBlobReference>().treasuresBlobReference;
                        ref var cards = ref treasuresBlobReference.Value.cards;
                        var random = SystemAPI.GetSingletonRW<RandomValueComponent>();
                        var randomIdx = random.ValueRW.Value.NextInt(0, 13);
                        var cardChosen = cards[randomIdx];
                        var treasureCard = SystemAPI.GetSingletonRW<TreasureCard>();
                        treasureCard.ValueRW.data = cardChosen.data;

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
