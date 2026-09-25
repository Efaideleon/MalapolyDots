using Unity.Cinemachine;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    // Constrain pipeline states, including standby cameras, before they enter a blend.
    public class BoardCameraClearance : CinemachineExtension
    {
        public float MinimumHeight = 3f;

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body && stage != CinemachineCore.Stage.Finalize) return;
            float height = state.GetCorrectedPosition().y;
            state.PositionCorrection += Vector3.up * Mathf.Max(0f, MinimumHeight - height);
        }
    }
}
