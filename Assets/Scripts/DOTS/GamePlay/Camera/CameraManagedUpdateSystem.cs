using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.Camera
{
    public struct MainCameraTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public partial struct CameraManageUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new MainCameraTransform 
            { 
                Position = default,
                Rotation = default
            });
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var transform in SystemAPI.Query<RefRW<MainCameraTransform>>().WithChangeFilter<MainCameraTransform>())
                UnityEngine.Camera.main.transform.SetPositionAndRotation(transform.ValueRO.Position, transform.ValueRO.Rotation);
        }
    }
}
