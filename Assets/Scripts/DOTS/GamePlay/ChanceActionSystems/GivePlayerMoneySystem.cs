using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.ChanceActionSystems
{
    [BurstCompile]
    public partial struct GivePlayerMoneySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChanceCardPicked>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<MoneyComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var card in SystemAPI.Query<RefRO<ChanceCardPicked>>().WithChangeFilter<ChanceCardPicked>())
            {
                if (card.ValueRO.id == 0)
                {
                    var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();
                    ref var money = ref SystemAPI.GetComponentRW<MoneyComponent>(player.entity).ValueRW;
                    money.Value += 1;
                }
            }
        }
    }
}
