using DOTS.Characters;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct RollAmountCountDown : IComponentData
    {
        public int Value;
    }

    [BurstCompile]
    public partial struct RollAmountCountDownSystem : ISystem
    {
        public ComponentLookup<RemainingMoves> rollCountLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountCountDown>();
            state.RequireForUpdate<CurrentActivePlayer>();

            state.EntityManager.CreateSingleton(new RollAmountCountDown { Value = default });
            rollCountLookup = SystemAPI.GetComponentLookup<RemainingMoves>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        { 
            rollCountLookup.Update(ref state);

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            
            if (!rollCountLookup.HasComponent(activePlayerEntity)) 
                return;

            if (rollCountLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value = rollCountLookup[activePlayerEntity].Value;
            }
        }
    }
}
