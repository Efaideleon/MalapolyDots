using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct RollAmountCountDown : IComponentData
    {
        public int Value;
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RollAmountCountDownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();

            state.EntityManager.CreateSingleton(new RollAmountCountDown { Value = default });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // TODO: this is running once.
            // rollCountLookup.Update(ref state);
            //
            // var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            //
            // if (!rollCountLookup.HasComponent(activePlayerEntity)) 
            //     return;
            //
            // if (rollCountLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            // {
            //     var rollValue = rollCountLookup[activePlayerEntity].Value;
            //     UnityEngine.Debug.Log($"[RollAmountCountDown] | Rolling.. : {rollValue}");
            //     SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value = rollValue;
            // }

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayerEntity == default)
                return;

            var rollValue = SystemAPI.GetComponent<RemainingMoves>(activePlayerEntity);
            SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value = rollValue.Value;
        }
    }
}
