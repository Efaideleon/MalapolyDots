using DOTS.DataComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Mediator
{
    using PivotRotation = GamePlay.CameraSystems.PivotRotation;
    public partial struct RotateTagPivotSystem : ISystem
    {
        private ComponentLookup<LocalToWorld> ltwLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PivotRotation>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<CurrentCameraManagedObject>();
            ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            ltwLookup.Update(ref state);

            var currCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
            if (currCamera.Camera == null) return;

            float3 camForward = currCamera.Camera.transform.forward;
            camForward.y = 0;
            camForward = math.normalize(camForward);

            var sharedTargetRotation = quaternion.LookRotationSafe(-camForward, math.up());
            new RotateTagPivots
            {
                targetWorldRotation = sharedTargetRotation,
                ltwLookup = ltwLookup,
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct RotateTagPivots : IJobEntity
    {
        public quaternion targetWorldRotation;
        [ReadOnly] public ComponentLookup<LocalToWorld> ltwLookup;

        public void Execute(ref LocalTransform tagTransform, in Parent parent, in PriceTagPivotTag _)
        {
            if (!ltwLookup.HasComponent(parent.Value)) return;

            var parentWorldRotation = ltwLookup[parent.Value].Rotation;

            tagTransform.Rotation = math.mul(math.inverse(parentWorldRotation), targetWorldRotation);
        }
    }
}
