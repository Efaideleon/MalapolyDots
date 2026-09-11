using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct RotateTagPivotSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PriceTagTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var camera = Camera.main;
            if (camera == null) return;

            // The visible tags are world-space roots, separate from the placement pivots.
            // Their digits face local -Z, so matching the camera rotation faces them
            // toward the viewer and keeps the text upright, including camera pitch.
            new RotateTagPivots
            {
                targetWorldRotation = camera.transform.rotation,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithNone(typeof(Parent))]
    public partial struct RotateTagPivots : IJobEntity
    {
        public quaternion targetWorldRotation;

        public void Execute(ref LocalTransform tagTransform, in PriceTagTag _)
        {
            tagTransform.Rotation = targetWorldRotation;
        }
    }
}
