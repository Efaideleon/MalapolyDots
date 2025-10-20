using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct PickRandomChanceCard : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<ChanceCardPicked>();
            state.RequireForUpdate<ChanceCardPicked>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<ChanceSpaceTag>(landedOnEntity.entity))
                    {
                        var chanceActionData = SystemAPI.GetBuffer<ChanceActionDataBuffer>(landedOnEntity.entity);
                        
                        // pick a random number
                        var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                        var numOfActions = chanceActionData.Length;
                        var randomNumber = randomData.ValueRW.Value.NextInt(0, numOfActions);

                        ref var cardPicked = ref SystemAPI.GetSingletonRW<ChanceCardPicked>().ValueRW;
                        cardPicked.id = chanceActionData[randomNumber].id;
                        cardPicked.msg = chanceActionData[randomNumber].msg;
                    }
                }
            }
        }
    }
}
