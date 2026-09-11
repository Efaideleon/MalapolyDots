using Unity.Cinemachine;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CinemachineCameraManager : MonoBehaviour
    {
        public static CinemachineCameraManager Instance { get; private set; }

        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera landingCamera;

        [Header("Target Group for Landing Shot")]
        [SerializeField] private CinemachineTargetGroup landingTargetGroup;

        private const int HIGH_PRIORITY = 20;
        private const int LOW_PRIORITY = 10;

        private CinemachineOrbitalFollow followOrbital;
        private float savedHorizontalAxis;
        private float savedVerticalAxis;
        private bool hasSavedFollowAxes;
        private int restoreFollowAxesFrames;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            followOrbital = followCamera != null ? followCamera.GetComponent<CinemachineOrbitalFollow>() : null;
            if (followCamera != null)
            {
                followCamera.BlendHint &= ~CinemachineCore.BlendHints.InheritPosition;
            }
        }

        private void LateUpdate()
        {
            if (restoreFollowAxesFrames <= 0)
            {
                return;
            }

            RestoreFollowAxes();
            restoreFollowAxesFrames--;
        }

        public void SwitchToFollowCamera()
        {
            Debug.Log($"[CinemachineCameraManager] | SwitchToFollowCamera");
            followCamera.Priority.Value = HIGH_PRIORITY;
            landingCamera.Priority.Value = LOW_PRIORITY;
            restoreFollowAxesFrames = 2;
            RestoreFollowAxes();
        }

        public void SwitchToLandingCamera(Transform playerTransform, Transform placeFrontTransform)
        {
            Debug.Log($"[CinemachineCameraManager] | SwitchToLandingCamera");
            SaveFollowAxes();
            // if (landingTargetGroup != null)
            // {
            //     landingTargetGroup.Targets.Clear();
            //     landingTargetGroup.AddMember(playerTransform, weight: 1.0f, radius: 1.0f);
            //     landingTargetGroup.AddMember(placeFrontTransform, weight: 3.0f, radius: 2f);
            //
            //     landingCamera.Follow = landingTargetGroup.transform;
            //     landingCamera.LookAt = landingTargetGroup.transform;
            // }

            landingCamera.Priority.Value = HIGH_PRIORITY;
            followCamera.Priority.Value = LOW_PRIORITY;
        }

        private void SaveFollowAxes()
        {
            if (followOrbital == null)
            {
                return;
            }

            savedHorizontalAxis = followOrbital.HorizontalAxis.Value;
            savedVerticalAxis = followOrbital.VerticalAxis.Value;
            hasSavedFollowAxes = true;
        }

        private void RestoreFollowAxes()
        {
            if (followOrbital == null || !hasSavedFollowAxes)
            {
                return;
            }

            followOrbital.HorizontalAxis.Value = savedHorizontalAxis;
            followOrbital.VerticalAxis.Value = savedVerticalAxis;
        }
    }
}
