using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

// ======================================================================
// This system updates the camera transform and translation using managed
// data.
// ======================================================================

namespace DOTS.GamePlay.CameraSystems
{
    public struct CachedMomentum : IComponentData
    {
        public float3 Momentum;
    }

    public partial struct CameraManageUpdateSystem : ISystem
    {
        // TODO: Extract the panning logic into a burstable system
        const float MaxFlingSpeed = 30f;
        const float DampingPerSecond = 5f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<ClickData>();
            state.EntityManager.CreateSingleton(new CachedMomentum { Momentum = default });
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            var click = SystemAPI.GetSingleton<ClickData>();
            ref var cache = ref SystemAPI.GetSingletonRW<CachedMomentum>().ValueRW;

            foreach (var transform in SystemAPI.Query<RefRW<MainCameraTransform>>().WithChangeFilter<MainCameraTransform>())
            {
                bool isFreeCamera = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value;
                if (!isFreeCamera)
                {
                    Camera.main.transform.SetPositionAndRotation(transform.ValueRO.Position, transform.ValueRO.Rotation);
                }
            }

            foreach (var translateData in SystemAPI.Query<RefRW<MainCameraTranslateData>>().WithChangeFilter<MainCameraTranslateData>())
            {
                var delta = translateData.ValueRO.Delta;

                if (click.Phase == InputActionPhase.Canceled || click.Phase == InputActionPhase.Started)
                {
                    SystemAPI.GetSingletonRW<MainCameraTranslateData>().ValueRW.Delta = 0;
                }

                if (click.Phase == InputActionPhase.Performed && math.lengthsq(delta) > 0)
                {
                    Camera.main.transform.Translate(translateData.ValueRO.Delta, translateData.ValueRO.Space);

                    // Velocity is a change in distance over a change in time, with both magnitude and direction.
                    var velocity = delta / dt;
                    var speed = math.length(velocity);

                    // Finding the unit vector for direction.
                    var direction = velocity / speed; 

                    if (speed > MaxFlingSpeed)
                        velocity = direction * MaxFlingSpeed;

                    cache.Momentum = velocity;
                }
                translateData.ValueRW.Delta = float3.zero;
            }
            
            if (click.Phase == InputActionPhase.Started)
            {
                cache.Momentum = float3.zero;
            }

            if (click.Phase != InputActionPhase.Performed && 
                click.Phase != InputActionPhase.Started &&
                math.lengthsq(cache.Momentum) > 0
            )
            {
                var move = cache.Momentum * dt;
                Camera.main.transform.Translate(move, Space.World);

                cache.Momentum = new float3(math.lerp(cache.Momentum, float3.zero, DampingPerSecond * dt));

                if (math.lengthsq(cache.Momentum) < 0.0002f)
                    cache.Momentum = float3.zero;
            }
        }
    }
}
