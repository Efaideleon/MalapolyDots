using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct PickRandomChanceCardSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<ChanceCardPicked>();
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<GhostChanceCardPicked>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<ChanceSpaceTag>();
            state.RequireForUpdate<ChanceActionDataBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    foreach (var (spaceLandedOn, chanceCardPicked) in SystemAPI.Query<RefRO<SpaceLandedOn>, RefRW<GhostChanceCardPicked>>().WithAll<ActivePlayer>())
                    {
                        if (SystemAPI.HasComponent<ChanceSpaceTag>(spaceLandedOn.ValueRO.entity))
                        {
                            var chanceActionData = SystemAPI.GetBuffer<ChanceActionDataBuffer>(spaceLandedOn.ValueRO.entity);

                            // pick a random number
                            var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                            var numOfActions = chanceActionData.Length;

                            UnityEngine.Debug.Log($"[PickRandomChanceCardSystem] | length of the buffer: {chanceActionData.Length}");

                            //var randomNumber = randomData.ValueRW.Value.NextInt(0, numOfActions);
                            var randomNumber = 0; 

                            chanceCardPicked.ValueRW.id = chanceActionData[randomNumber].id;
                            chanceCardPicked.ValueRW.msg = chanceActionData[randomNumber].msg;
                        }
                    }
                }
            }
        }
    }
}
