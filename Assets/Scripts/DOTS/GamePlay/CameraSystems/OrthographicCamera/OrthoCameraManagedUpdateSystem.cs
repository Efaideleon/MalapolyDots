using Unity.Entities;
using UnityEngine;

// ======================================================================
// This system updates the camera transform and translation using managed
// data.
// ======================================================================

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    ///<summary>
    ///This system uses and updates managed properties from the `Camera.main`.
    ///</summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct OrthoCameraManageUpdateSystem : ISystem
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

            foreach (var translateData in SystemAPI.Query<RefRO<MainCameraTranslateData>>().WithChangeFilter<MainCameraTranslateData>())
            {
                Camera.main.transform.Translate(translateData.ValueRO.Delta, Space.World);
            }

            foreach (var fieldOfView in SystemAPI.Query<RefRO<CameraFieldOfView>>().WithChangeFilter<CameraFieldOfView>())
            {
                Camera.main.orthographicSize = fieldOfView.ValueRO.Value;
            }
        }
    }
}
