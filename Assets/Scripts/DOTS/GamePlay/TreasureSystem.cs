using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay.PropertyAnimations;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public struct TreasureCard : IComponentData
    {
        public FixedString64Bytes data;
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TreasureSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new TreasureCard { data = default });
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<TreasureAnimationBuffer>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<ActivePlayer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    foreach (var spaceLandedOn in SystemAPI.Query<RefRO<SpaceLandedOn>>().WithAll<GhostOwnerIsLocal, ActivePlayer>())
                    {
                        var landedOnEntity = spaceLandedOn.ValueRO.entity;
                        if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity))
                        {
                            var cards = SystemAPI.GetBuffer<TreasureCardsBuffer>(landedOnEntity);
                            var random = SystemAPI.GetSingletonRW<RandomValueComponent>();
                            var randomIdx = random.ValueRW.Value.NextInt(0, cards.Length);
                            var cardChosen = cards[randomIdx];
                            var treasureCard = SystemAPI.GetSingletonRW<TreasureCard>();
                            treasureCard.ValueRW.data = cardChosen.msg;

                            //UnityEngine.Debug.Log($"[TreasureSystem] | cardChosen msg: {treasureCard.ValueRO.data.ToString()}");

                            // Reset that treasure's open animation.
                            SystemAPI.GetComponentRW<CurrentTreasureAnimation>(landedOnEntity).ValueRW.Value = TreasureAnimation.Open;
                            SystemAPI.GetComponentRW<AnimationPlayState>(landedOnEntity).ValueRW.Value = PlayState.Playing;
                            UnityEngine.Debug.Log($"[TreasureSystem] | Set Animation to open");
                        }
                    }
                }
            }
        }
    }
}
