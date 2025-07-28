using Unity.Entities;
using UnityEngine;

// ======================================================================
// This system updates the camera transform and translation using managed
// data.
// ======================================================================

namespace DOTS.GamePlay.CameraSystems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
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

            foreach (var translateData in SystemAPI.Query<RefRO<MainCameraTranslateData>>().WithChangeFilter<MainCameraTranslateData>())
            {
                Camera.main.transform.Translate(translateData.ValueRO.Delta, Space.World);
            }
        }
    }
}
