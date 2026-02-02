using DOTS.GamePlay.CameraSystems.Components;
using Input;
using Unity.Entities;
using UnityEngine;

// ======================================================================
// This system updates the camera transform and translation using managed
// data.
// ======================================================================

#nullable enable
namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    ///<summary>
    ///This system uses and updates managed properties from the `Camera.main`.
    ///</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct OrthoCameraManageUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<OrthographicCameraObject>();
            //state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<CameraFieldOfView>();
            //state.RequireForUpdate<ClickData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // // TODO: Use OrthographicCameraObject instead of Camera.main
            // var cameraRef = SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>();
            // if (cameraRef.camera == null) return;
            //
            // foreach (var transform in SystemAPI.Query<RefRW<MainCameraTransform>>().WithChangeFilter<MainCameraTransform>())
            // {
            //     // bool isFreeCamera = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value;
            //     // if (!isFreeCamera)
            //     // {
            //     cameraRef.camera.transform.localRotation = transform.ValueRO.Rotation;
            //     // }
            // }
            //
            // //TODO: buggy with new pivot
            // foreach (var translateData in SystemAPI.Query<RefRO<MainCameraTranslateData>>().WithChangeFilter<MainCameraTranslateData>())
            // {
            //     cameraRef.camera.transform.Translate(translateData.ValueRO.Delta, Space.World);
            // }
        }
    }
}
