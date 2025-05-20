using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace DOTS.GamePlay.CameraSystems
{
    [BurstCompile]
    public partial struct CameraSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<CurrentPlayerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var mainCameraTransform = SystemAPI.GetSingletonRW<MainCameraTransform>();
            var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();
            if (SystemAPI.HasComponent<LocalTransform>(player.entity))
            {
                var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(player.entity);
                var offset = new float3(17.54f, 13.87f, -21.8f);
                mainCameraTransform.ValueRW.Position = playerLocalTransform.Position + offset;
                float3 forward = math.normalize(playerLocalTransform.Position - mainCameraTransform.ValueRO.Position);
                var up = new float3(0, 1, 0);
                var quaternionToLookAtPlayer = quaternion.LookRotationSafe(forward, up);
                mainCameraTransform.ValueRW.Rotation = quaternionToLookAtPlayer;
                //state.Enabled = false;
                // mainCameraTransform.ValueRW.Position.x += deltaTime * 1;
            }
        }
    }
}
