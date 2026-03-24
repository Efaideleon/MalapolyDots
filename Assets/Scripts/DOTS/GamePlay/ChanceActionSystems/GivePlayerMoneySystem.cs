using Assets.Scripts.DOTS.Characters;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.ChanceActionSystems
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GivePlayerMoneySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GhostChanceCardPicked>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<GhostMoneyComponet>();
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
                    foreach (var (money, card) in SystemAPI.Query<RefRW<GhostMoneyComponet>, RefRO<GhostChanceCardPicked>>().WithAll<ActivePlayer>())
                    {
                        if (card.ValueRO.id == 0)
                        {
                            // TODO: what if the entity is null;
                            money.ValueRW.Value += 1;
                        }
                    }
                }

                buffer.Clear();
            }
        }
    }
}
