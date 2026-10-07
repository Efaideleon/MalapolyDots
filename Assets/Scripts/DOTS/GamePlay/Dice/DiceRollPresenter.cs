using System;
using UnityEngine;

namespace DOTS.GamePlay
{
    [DefaultExecutionOrder(10000)]
    public sealed class DiceRollPresenter : MonoBehaviour
    {
        public Transform Visuals;
        public ChosenValueDie FirstDie, SecondDie;
        public bool FollowCamera = true;
        public Camera PresentationCamera;
        [Tooltip("World-space height of the board's flat floor.")]
        public float FloorHeight;
        [Min(.1f)] public float FloorDieSize = 2f;
        [Range(1, 6)] public int PreviewFirst = 3;
        [Range(1, 6)] public int PreviewSecond = 5;
        public bool PlayOnStart;
        public DiceResultOverlay ResultOverlay;
        public event Action<uint> Settled;
        public bool IsRolling { get; private set; }
        public uint Sequence { get; private set; }
        float elapsed;
        bool visible;
        Vector3 floorPosition;
        Quaternion floorRotation;
        float floorScale;
        Camera View => PresentationCamera != null ? PresentationCamera : Camera.main;

        void Start() { if (PlayOnStart) PreviewRoll(); }

        [ContextMenu("Roll chosen values")]
        public void PreviewRoll() => Play(PreviewFirst, PreviewSecond, Sequence + 1);

        public void Play(int first, int second, uint sequence = 1)
        {
            if (first < 1 || first > 6 || second < 1 || second > 6)
                throw new ArgumentOutOfRangeException(nameof(first), "Both dice must be between 1 and 6.");
            Sequence = sequence;
            elapsed = 0;
            IsRolling = visible = true;
            ResultOverlay?.Hide();
            Visuals.gameObject.SetActive(true);
            PlaceOnFloor();
            FirstDie.Begin(first, new Vector3(-1.12f, 0, .12f), sequence * 73856093u + 17u, -1);
            SecondDie.Begin(second, new Vector3(1.12f, 0, -.08f), sequence * 19349663u + 31u, 1);
            SampleSequence(0);
        }

        void PlaceOnFloor()
        {
            floorPosition = new Vector3(transform.position.x, FloorHeight, transform.position.z);
            floorRotation = Quaternion.identity;
            floorScale = FollowCamera ? FloorDieSize : 1;
            var camera = View;
            if (FollowCamera && camera != null)
            {
                // Capture a fixed patch of floor once; camera motion cannot drag the rolling dice.
                var ray = camera.ViewportPointToRay(new Vector3(.5f, .38f, 0));
                var floor = new Plane(Vector3.up, Vector3.up * FloorHeight);
                if (floor.Raycast(ray, out float distance)) floorPosition = ray.GetPoint(distance);
                else
                {
                    var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
                    floorPosition = camera.transform.position + forward * 12;
                    floorPosition.y = FloorHeight;
                }
                floorRotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
            }
            transform.SetPositionAndRotation(floorPosition, floorRotation);
            transform.localScale = Vector3.one * floorScale;
        }

        void LateUpdate()
        {
            if (Assets.Common.SoloSession.Paused) return;
            if (!visible) return;
            elapsed += Time.unscaledDeltaTime;
            SampleSequence(elapsed);
            if (IsRolling && elapsed >= DiceRollValues.AnimationSeconds)
            {
                IsRolling = false;
                Settled?.Invoke(Sequence);
            }
            if (elapsed >= DiceRollValues.AnimationSeconds + DiceRollValues.ResultHoldSeconds) Hide();
        }

        // Shared by runtime and Editor verification, including the exact landing and reveal boundaries.
        public void SampleSequence(float seconds)
        {
            float roll = Mathf.Clamp01(seconds / DiceRollValues.FloorRollSeconds);
            float pull = Mathf.Clamp01((seconds - DiceRollValues.FloorRollSeconds - DiceRollValues.FloorPauseSeconds) / DiceRollValues.PullSeconds);
            float ease = pull * pull * (3 - 2 * pull);
            transform.SetPositionAndRotation(floorPosition, floorRotation);
            transform.localScale = Vector3.one * floorScale;
            FirstDie.Sample(roll);
            SecondDie.Sample(roll);
            SetShadow(FirstDie, pull <= 0);
            SetShadow(SecondDie, pull <= 0);
            var camera = View;
            if (FollowCamera && camera != null && pull > 0)
            {
                float depth = camera.nearClipPlane + 3;
                float height = camera.orthographic ? camera.orthographicSize * 2 :
                    2 * depth * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                float scale = Mathf.Min(height * camera.aspect * .52f / 3.5f, height * .22f);
                var rotation = camera.transform.rotation * Quaternion.Euler(-82, 0, 0);
                // Offset the root so the die centers sit exactly at the screen anchor.
                var target = camera.ViewportToWorldPoint(new Vector3(.5f, .57f, depth)) - rotation * (Vector3.up * .5f * scale);
                var position = Vector3.Lerp(floorPosition, target, ease);
                position.y += Mathf.Sin(pull * Mathf.PI) * floorScale * .8f;
                transform.SetPositionAndRotation(position, Quaternion.Slerp(floorRotation, rotation, ease));
                transform.localScale = Vector3.one * Mathf.Lerp(floorScale, scale, ease);
                FirstDie.transform.localPosition = Vector3.Lerp(FirstDie.transform.localPosition, new Vector3(-.86f, .5f, 0), ease);
                SecondDie.transform.localPosition = Vector3.Lerp(SecondDie.transform.localPosition, new Vector3(.86f, .5f, 0), ease);
            }
            if (seconds >= DiceRollValues.AnimationSeconds)
                ResultOverlay?.Show(FirstDie.Value, SecondDie.Value);
            else ResultOverlay?.Hide();
        }

        static void SetShadow(ChosenValueDie die, bool show)
        {
            if (die.ContactShadow != null) die.ContactShadow.gameObject.SetActive(show);
        }

        public void Hide()
        {
            IsRolling = visible = false;
            ResultOverlay?.Hide();
            if (Visuals != null) Visuals.gameObject.SetActive(false);
        }
    }
}
