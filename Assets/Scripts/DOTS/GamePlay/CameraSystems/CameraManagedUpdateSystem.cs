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
    public partial struct CameraManageUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<ClickData>();
        }

        public void OnUpdate(ref SystemState state)
        {
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
                Debug.Log($"[CameraManageUpdateSystem] | translateData.Delta: {translateData.ValueRO.Delta}");
                Camera.main.transform.Translate(translateData.ValueRO.Delta, Space.World);
            }
        }
    }
}
