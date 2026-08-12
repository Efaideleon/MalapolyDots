using System;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay.PropertyAnimations;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TreasureSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<TreasureAnimationBuffer>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<ActivePlayer>();
            state.RequireForUpdate<GhostTreasureCardPicked>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    foreach (var (spaceLandedOn, treasureCardPicked) in SystemAPI.Query<RefRO<SpaceLandedOn>, RefRW<GhostTreasureCardPicked>>().WithAll<ActivePlayer>())
                    {
                        var landedOnEntity = spaceLandedOn.ValueRO.entity;

                        // If landed on a treasure spot.
                        if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity))
                        {
                            var cards = SystemAPI.GetBuffer<TreasureCardsBuffer>(landedOnEntity);
                            var random = SystemAPI.GetSingletonRW<RandomValueComponent>();
                            var randomIdx = random.ValueRW.Value.NextInt(0, cards.Length);
                            var cardChosen = cards[randomIdx];
                            treasureCardPicked.ValueRW.id = cardChosen.id;
                            treasureCardPicked.ValueRW.msg = cardChosen.msg;
                            treasureCardPicked.ValueRW.amount = cardChosen.amount;

                            // Give treasure money
                            foreach (var money in SystemAPI.Query<RefRW<GhostMoneyComponet>>().WithAll<ActivePlayer>())
                            {
                                money.ValueRW.Value += treasureCardPicked.ValueRO.amount;
                            }

                            // Reset that treasure's open animation.
                            SystemAPI.GetComponentRW<CurrentTreasureAnimation>(landedOnEntity).ValueRW.Value = TreasureAnimation.Open;
                            SystemAPI.GetComponentRW<AnimationPlayState>(landedOnEntity).ValueRW.Value = PlayState.Playing;
                        }
                    }
                }
            }
        }
    }
}
