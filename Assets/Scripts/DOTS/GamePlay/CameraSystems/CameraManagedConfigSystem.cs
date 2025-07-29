using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraManagedConfigSystem : ISystem
    {
        private const int CameraOrthographicSize = 14;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<CameraFieldOfView>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = CameraOrthographicSize;
            // Would the camera have a field of view in orthographic?
            // TODO: or does we need an if statement to ensure that it is in orthographic?
            SystemAPI.GetSingletonRW<CameraFieldOfView>().ValueRW.Value = CameraOrthographicSize;
        }
    }
}
