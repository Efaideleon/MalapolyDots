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
            var playerEntity= SystemAPI.GetSingleton<CurrentPlayerComponent>().entity;
            var player = SystemAPI.GetComponent<LocalTransform>(playerEntity);

            var currCameraData = SystemAPI.GetSingleton<CurrentCameraData>();
            ref var cam = ref SystemAPI.GetSingletonRW<MainCameraTransform>().ValueRW;

            cam.Position = player.Position + currCameraData.Offset;
            float3 forward = math.normalize(player.Position - cam.Position);
            cam.Rotation = quaternion.LookRotationSafe(forward, math.up());
        }
    }
}
