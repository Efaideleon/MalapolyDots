using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This systems matches the position of the player to the position of the pivot.
    /// </summary>
    public partial struct PivotFollowPlayerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<PivotTransform>();
            state.RequireForUpdate<CurrentPlayerComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();

            if (!SystemAPI.HasComponent<LocalTransform>(player.entity)) return;

            var playerTransform = SystemAPI.GetComponent<LocalTransform>(player.entity);

            SystemAPI.GetSingletonRW<PivotTransform>().ValueRW.Position = playerTransform.Position;
        }
    }

    public struct PivotTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }
}
