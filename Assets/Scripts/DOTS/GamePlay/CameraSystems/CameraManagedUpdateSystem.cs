using Unity.Entities;
using UnityEngine;

// ======================================================================
// This system updates the camera transform and translation using managed
// data.
// ======================================================================

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CameraManageUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<CameraFieldOfView>();
            state.RequireForUpdate<ClickData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (Camera.main == null) return;

            foreach (var transform in SystemAPI.Query<RefRW<MainCameraTransform>>().WithChangeFilter<MainCameraTransform>())
            {
                bool isFreeCamera = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value;
                if (!isFreeCamera)
                {
                    Camera.main.transform.SetPositionAndRotation(transform.ValueRO.Position, transform.ValueRO.Rotation);
                }
            }

            // TODO: Create a foreach to change from orthographic to perspective.
            // TODO: Will need a componentData to keep track of this.

            foreach (var translateData in SystemAPI.Query<RefRO<MainCameraTranslateData>>().WithChangeFilter<MainCameraTranslateData>())
            {
                Camera.main.transform.Translate(translateData.ValueRO.Delta, Space.World);
            }

            foreach (var fieldOfView in SystemAPI.Query<RefRO<CameraFieldOfView>>().WithChangeFilter<CameraFieldOfView>())
            {
                // TODO: will need a if statment to check if we are in orthographic or perspective
                Camera.main.orthographicSize = fieldOfView.ValueRO.Value;
            }
        }
    }
}
