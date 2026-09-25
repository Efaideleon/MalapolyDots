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
    [UpdateAfter(typeof(GamePlaySystem))]
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
            using var moneyCommands = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    foreach (var (spaceLandedOn, chanceCardPicked, resolved) in SystemAPI.Query<RefRO<SpaceLandedOn>, RefRW<GhostChanceCardPicked>, RefRW<LandingPaymentResolved>>().WithAll<ActivePlayer>())
                    {
                        if (resolved.ValueRO.Value) continue;
                        if (SystemAPI.HasComponent<ChanceSpaceTag>(spaceLandedOn.ValueRO.entity))
                        {
                            var chanceActionData = SystemAPI.GetBuffer<ChanceActionDataBuffer>(spaceLandedOn.ValueRO.entity);

                            if (chanceActionData.Length == 0) continue;
                            resolved.ValueRW.Value = true;
                            chanceCardPicked.ValueRW.DrawSequence++;

                            // pick a random number
                            var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                            var numOfActions = chanceActionData.Length;

                            UnityEngine.Debug.Log($"[PickRandomChanceCardSystem] | length of the buffer: {chanceActionData.Length}");

                            var randomNumber = randomData.ValueRW.Value.NextInt(0, numOfActions);
                            //var randomNumber = 0; 

                            chanceCardPicked.ValueRW.id = chanceActionData[randomNumber].id;
                            chanceCardPicked.ValueRW.msg = chanceActionData[randomNumber].msg;
                            chanceCardPicked.ValueRW.amount = chanceActionData[randomNumber].amount;
                            foreach (var (money, player) in SystemAPI.Query<RefRW<GhostMoneyComponet>>().WithAll<ActivePlayer>().WithEntityAccess())
                            {
                                money.ValueRW.Value += chanceCardPicked.ValueRO.amount;
                                MoneyFeedback.Send(moneyCommands, state.EntityManager, player, chanceCardPicked.ValueRO.amount, MoneyChangeReason.Card);
                            }
                        }
                    }
                }
            }
            moneyCommands.Playback(state.EntityManager);
        }
    }
}
