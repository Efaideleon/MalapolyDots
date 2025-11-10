using DOTS.Characters;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct PlayerToCameraAngle : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<PivotTransform>();
            state.EntityManager.CreateSingleton(new PlayerToCameraAngleData { Map = new(6, Allocator.Persistent) });
        }

        public void OnUpdate(ref SystemState state) 
        { 
            state.Enabled = false;
            var map = SystemAPI.GetSingletonRW<PlayerToCameraAngleData>();
            var pivot = SystemAPI.GetSingleton<PivotTransform>();

            foreach (var id in SystemAPI.Query<RefRO<PlayerID>>())
            {
                UnityEngine.Debug.Log($"[PlayerToCameraAngle] | id: {id.ValueRO.Value} rotation: {pivot.Rotation}");
                map.ValueRW.Map.TryAdd(id.ValueRO.Value, pivot.Rotation);
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            var playerToCameraAngleData = SystemAPI.GetSingletonRW<PlayerToCameraAngleData>();
            playerToCameraAngleData.ValueRO.Map.Clear();
            playerToCameraAngleData.ValueRO.Map.Dispose();
        }
    }

    // TODO: Rename to PlayerToCameraRotationData
    public struct PlayerToCameraAngleData : IComponentData
    {
        public NativeHashMap<int, quaternion> Map;
    }
}
