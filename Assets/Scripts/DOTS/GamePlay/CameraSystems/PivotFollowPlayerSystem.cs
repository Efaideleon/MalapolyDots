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
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<PivotPosition>(),
                ComponentType.ReadOnly<PivotRotation>(),
                ComponentType.ReadOnly<PivotTransformTag>()
            });

            SystemAPI.SetComponent(entity, new PivotPosition { Value = default });
            SystemAPI.SetComponent(entity, new PivotRotation { Value = quaternion.identity });
            SystemAPI.SetComponent(entity, new PivotTransformTag { });

            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<CurrentCameraData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();

            if (!SystemAPI.HasComponent<LocalTransform>(player.entity)) return;

            var playerTransform = SystemAPI.GetComponent<LocalTransform>(player.entity);

            SystemAPI.GetSingletonRW<PivotPosition>().ValueRW.Value = playerTransform.Position;
        }
    }

    public struct PivotPosition : IComponentData
    {
        public float3 Value;
    }

    public struct PivotRotation : IComponentData
    {
        public quaternion Value;
    }

    public struct PivotTransformTag : IComponentData
    { }
}
