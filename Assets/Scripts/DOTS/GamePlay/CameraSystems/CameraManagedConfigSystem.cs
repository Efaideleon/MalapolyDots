using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraManagedConfigSystem : ISystem
    {
        private const int CameraOrthographicSize = 12;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = CameraOrthographicSize;
        }
    }
}
