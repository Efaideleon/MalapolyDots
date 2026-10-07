using System.Collections.Generic;
using Assets.Common;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CinemachineCameraManager : MonoBehaviour
    {
        public static CinemachineCameraManager Instance { get; private set; }

        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera followCamera;
        [SerializeField] private CinemachineCamera landingCamera;

        [Header("Landing Shot")]
        [SerializeField] private Vector3 landingOffset = new Vector3(0f, 12f, 16f);

        [Header("Target Group for Landing Shot")]
        [SerializeField] private CinemachineTargetGroup landingTargetGroup;

        [Header("Board Clearance")]
        [SerializeField] private float boardHeight = 0f;
        [SerializeField, Min(1f)] private float minimumCameraHeight = 3f;

        private const int HIGH_PRIORITY = 20;
        private const int LOW_PRIORITY = 10;

        private CinemachineOrbitalFollow followOrbital;
        private float savedHorizontalAxis;
        private float savedVerticalAxis;
        private bool hasSavedFollowAxes;
        private int restoreFollowAxesFrames;
        private BoardCameraInput boardInput;
        private bool reducedMotion;
        private readonly Dictionary<CinemachineBrain, float> originalBlendTimes = new();

        [Header("Money Feedback")]
        [SerializeField, Min(0.2f)] private float moneyShotDuration = 1.1f;
        [SerializeField, Min(0.5f)] private float moneyHeadHeight = 3f;
        [SerializeField] private Vector3 moneyShotOffset = new Vector3(0f, 7f, 11f);
        private CinemachineCamera moneyCamera;
        private Vector3 moneyViewOffset;
        public float MoneyShotDuration => moneyShotDuration;
        public float MoneyHeadHeight => moneyHeadHeight;
        public bool IsShowingMoney { get; private set; }

        public void ShowMoneyShot(Vector3 playerPosition)
        {
            if (GamePreferences.Current.ReducedCameraMotion) return;
            if (moneyCamera == null)
            {
                var shot = new GameObject("Money transaction camera");
                shot.transform.SetParent(transform, false);
                moneyCamera = shot.AddComponent<CinemachineCamera>();
                if (followCamera != null)
                {
                    moneyCamera.Lens = followCamera.Lens;
                    moneyCamera.OutputChannel = followCamera.OutputChannel;
                }
                moneyCamera.Lens.OrthographicSize = 7f;
                moneyCamera.Lens.FieldOfView = 45f;
                ConfigureCamera(moneyCamera);
            }
            if (!IsShowingMoney)
            {
                var view = Camera.main;
                var heading = view != null ? view.transform.eulerAngles.y : 0f;
                moneyViewOffset = Quaternion.Euler(0f, heading, 0f) * new Vector3(moneyShotOffset.x, moneyShotOffset.y, -moneyShotOffset.z);
            }
            IsShowingMoney = true;
            moneyCamera.Priority.Value = 100;
            UpdateMoneyShot(playerPosition);
            moneyCamera.PreviousStateIsValid = false;
            ResetMoneyBrain();
        }

        public void UpdateMoneyShot(Vector3 playerPosition)
        {
            if (!IsShowingMoney || moneyCamera == null) return;
            var focus = playerPosition + Vector3.up * (moneyHeadHeight * 0.5f);
            var position = focus + moneyViewOffset;
            position.y = Mathf.Max(position.y, boardHeight + minimumCameraHeight);
            moneyCamera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(focus - position));
        }

        public void EndMoneyShot()
        {
            if (!IsShowingMoney) return;
            IsShowingMoney = false;
            if (moneyCamera != null) moneyCamera.Priority.Value = -100;
            if (followCamera != null) followCamera.PreviousStateIsValid = false;
            if (landingCamera != null) landingCamera.PreviousStateIsValid = false;
            ResetMoneyBrain();
        }

        private void ResetMoneyBrain()
        {
            for (int i = 0; i < CinemachineBrain.ActiveBrainCount; i++)
            {
                var brain = CinemachineBrain.GetActiveBrain(i);
                if (moneyCamera != null && brain.IsValidChannel(moneyCamera)) brain.ResetState();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            followOrbital = followCamera != null ? followCamera.GetComponent<CinemachineOrbitalFollow>() : null;
            ConfigureCamera(followCamera);
            ConfigureLandingCamera(landingCamera);
            ConfigureCamera(landingCamera);
            boardInput = gameObject.AddComponent<BoardCameraInput>();
            boardInput.Initialize(followCamera, landingCamera);
            GamePreferences.Changed += ApplyMotionPreference;
            ApplyMotionPreference();
            SwitchToFollowCamera();
        }

        private void ApplyMotionPreference()
        {
            bool reduce = GamePreferences.Current.ReducedCameraMotion;
            for (int i = 0; i < CinemachineBrain.ActiveBrainCount; i++)
            {
                var brain = CinemachineBrain.GetActiveBrain(i);
                if (followCamera == null || !brain.IsValidChannel(followCamera)) continue;
                if (!originalBlendTimes.ContainsKey(brain)) originalBlendTimes.Add(brain, brain.DefaultBlend.Time);
                brain.DefaultBlend.Time = reduce ? 0f : originalBlendTimes[brain];
            }
            if (reducedMotion == reduce) return;
            reducedMotion = reduce;
            if (reduce) { EndMoneyShot(); CutToPlayer(); }
        }

        public void ResetPlayerView()
        {
            boardInput?.ResetView();
            hasSavedFollowAxes = false;
            CutToPlayer();
        }

        private void ConfigureLandingCamera(CinemachineCamera camera)
        {
            if (camera == null) return;

            // PositionComposer moves in camera space after aiming. Paired with a
            // RotationComposer aiming at a different offset, it continually moves
            // the camera away from the aim solution (especially at board clearance).
            // Anchor the shot to the property's heading instead, then aim from there.
            var composer = camera.GetComponent<CinemachinePositionComposer>();
            var damping = composer != null ? composer.Damping : Vector3.one;
            // Cinemachine caches the first component per stage even when disabled.
            // Remove the old body immediately, before this frame's pipeline runs.
            if (composer != null) DestroyImmediate(composer);

            var follow = camera.GetComponent<CinemachineFollow>();
            if (follow == null) follow = camera.gameObject.AddComponent<CinemachineFollow>();
            follow.enabled = true;
            follow.FollowOffset = landingOffset;
            follow.TrackerSettings.BindingMode = BindingMode.LockToTargetWithWorldUp;
            follow.TrackerSettings.PositionDamping = damping;
            camera.PreviousStateIsValid = false;
        }

        private void ConfigureCamera(CinemachineCamera camera)
        {
            if (camera == null) return;
            // A straight positional blend cannot arc below two safe endpoints.
            // Freeze the outgoing shot so a new player's targets don't pull it around.
            camera.BlendHint = CinemachineCore.BlendHints.FreezeWhenBlendingOut;
            var clearance = camera.GetComponent<BoardCameraClearance>();
            if (clearance == null) clearance = camera.gameObject.AddComponent<BoardCameraClearance>();
            clearance.MinimumHeight = boardHeight + minimumCameraHeight;
            camera.PreviousStateIsValid = false;
        }

        public void CutToPlayer()
        {
            SwitchToFollowCamera();
            if (IsShowingMoney) return;
            // Reset position and aim damping for an immediate shot at the new player.
            if (followCamera != null) followCamera.PreviousStateIsValid = false;
            if (landingCamera != null) landingCamera.PreviousStateIsValid = false;
            for (int i = 0; i < CinemachineBrain.ActiveBrainCount; i++)
            {
                var brain = CinemachineBrain.GetActiveBrain(i);
                if (followCamera != null && brain.IsValidChannel(followCamera))
                    brain.ResetState();
            }
        }

        private void OnDestroy()
        {
            GamePreferences.Changed -= ApplyMotionPreference;
            foreach (var pair in originalBlendTimes)
                if (pair.Key != null) pair.Key.DefaultBlend.Time = pair.Value;
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            if (originalBlendTimes.Count < CinemachineBrain.ActiveBrainCount) ApplyMotionPreference();
            if (restoreFollowAxesFrames <= 0)
            {
                return;
            }

            RestoreFollowAxes();
            restoreFollowAxesFrames--;
        }

        public void SwitchToFollowCamera()
        {
            if (followCamera == null || landingCamera == null) return;
            followCamera.Priority.Value = HIGH_PRIORITY;
            landingCamera.Priority.Value = LOW_PRIORITY;
            restoreFollowAxesFrames = 2;
            RestoreFollowAxes();
        }

        public void SwitchToLandingCamera(Transform playerTransform, Transform placeFrontTransform)
        {
            if (GamePreferences.Current.ReducedCameraMotion) { SwitchToFollowCamera(); return; }
            if (followCamera == null || landingCamera == null) return;
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
