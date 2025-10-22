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
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ChanceBufferEvent>>().WithChangeFilter<ChanceBufferEvent>())
            {
                if (buffer.Length < 1)
                    continue;

                foreach (var _ in buffer)
                {
                    var card = SystemAPI.GetSingleton<ChanceCardPicked>();
                    if (card.id == 0)
                    {
                        var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();
                        ref var money = ref SystemAPI.GetComponentRW<MoneyComponent>(player.entity).ValueRW;
                        money.Value += 1;
                    }
                }

                buffer.Clear();
            }
        }
    }
}
