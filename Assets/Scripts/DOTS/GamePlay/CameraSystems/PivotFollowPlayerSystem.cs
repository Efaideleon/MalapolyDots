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
        }

        public void OnUpdate(ref SystemState state)
        {
            var currPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();

            if (!SystemAPI.HasComponent<LocalTransform>(currPlayer.entity)) return;

            var currPlayerTransform = SystemAPI.GetComponent<LocalTransform>(currPlayer.entity);

            SystemAPI.GetSingletonRW<PivotPosition>().ValueRW.Value = currPlayerTransform.Position;
        }
    }

    ///<summary>Stores the position for the pivot.</summary>
    public struct PivotPosition : IComponentData
    {
        public float3 Value;
    }

    ///<summary>Stores the rotation for the pivot.</summary>
    public struct PivotRotation : IComponentData
    {
        public quaternion Value;
    }

    ///<summary>Tag to identify the pivot.</summary>
    public struct PivotTransformTag : IComponentData
    { }
}
