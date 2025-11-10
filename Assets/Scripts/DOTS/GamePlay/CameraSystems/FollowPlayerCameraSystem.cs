using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay.CameraSystems
{
    // <summary>
    // Set the current camera to follow the current player.
    // </summary>
    [BurstCompile]
    public partial struct FollowPlayerCameraSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<CurrentCameraData>();
            state.RequireForUpdate<MainCameraTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // var pivot = SystemAPI.GetSingleton<PivotTransform>();
            //
            // var currCameraData = SystemAPI.GetSingleton<CurrentCameraData>();
            // ref var cam = ref SystemAPI.GetSingletonRW<MainCameraTransform>().ValueRW;
            //
            // // It only makes sure that the camera is looking at the player now.
            // // Doesn't update the position of the camera
            // var origin = float3.zero;
            // var camPosition = origin + currCameraData.LocalPosition;
            // float3 forward = math.normalize(origin - camPosition);
            // cam.Rotation = quaternion.LookRotationSafe(forward, math.up());
        }
    }
}
