using Assets.Common;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DOTS.GamePlay.CameraSystems
{
    public sealed class BoardCameraView
    {
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public float Distance { get; private set; } = 1f;

        public void Rotate(Vector2 delta, GamePreferencesData settings)
        {
            Yaw = Mathf.Repeat(Yaw - delta.x * 0.18f * settings.CameraSensitivity + 180f, 360f) - 180f;
            Pitch = Mathf.Clamp(Pitch + delta.y * 0.14f * settings.CameraSensitivity *
                (settings.InvertVertical ? -1f : 1f), -15f, 30f);
        }

        public void Zoom(float ratio, float sensitivity)
        {
            if (ratio <= 0f || float.IsNaN(ratio) || float.IsInfinity(ratio)) return;
            Distance = Mathf.Clamp(Distance * Mathf.Pow(ratio, sensitivity), 0.7f, 1.8f);
        }

        public Vector3 Offset(Vector3 baseline) => Quaternion.Euler(Pitch, Yaw, 0f) * baseline * Distance;
        public void Reset() { Yaw = Pitch = 0f; Distance = 1f; }
    }

    public sealed class BoardCameraInput : MonoBehaviour
    {
        readonly BoardCameraView view = new();
        CinemachineCamera followCamera, landingCamera;
        CinemachineFollow follow, landing;
        Vector3 followOffset, landingOffset;
        float followSize, landingSize;
        bool mouseDragging, touchDragging, trackingTouches;
        int firstTouchId, secondTouchId;
        Vector2 lastMouse, lastCenter;
        float lastDistance;

        public void Initialize(CinemachineCamera following, CinemachineCamera revealing)
        {
            followCamera = following;
            landingCamera = revealing;
            follow = following != null ? following.GetComponent<CinemachineFollow>() : null;
            landing = revealing != null ? revealing.GetComponent<CinemachineFollow>() : null;
            if (follow != null) followOffset = follow.FollowOffset;
            if (landing != null) landingOffset = landing.FollowOffset;
            if (following != null) followSize = following.Lens.OrthographicSize;
            if (revealing != null) landingSize = revealing.Lens.OrthographicSize;
        }

        void Update()
        {
            if (!GameSettingsController.HasActiveMatch || GameSettingsController.IsOpen || SoloSession.Paused ||
                !Application.isFocused)
            {
                mouseDragging = touchDragging = trackingTouches = false;
                return;
            }
            var settings = GamePreferences.Current;
            float pixelsToReference = 720f / Mathf.Max(1, Screen.height);
            var screen = Touchscreen.current;
            int count = 0, idA = 0, idB = 0;
            Vector2 a = default, b = default;
            if (screen != null)
                foreach (var touch in screen.touches)
                    if (touch.press.isPressed)
                    {
                        if (count == 0) { a = touch.position.ReadValue(); idA = touch.touchId.ReadValue(); }
                        if (count == 1) { b = touch.position.ReadValue(); idB = touch.touchId.ReadValue(); }
                        count++;
                    }
            if (count == 2)
            {
                mouseDragging = false;
                Vector2 center = (a + b) * 0.5f;
                float distance = Vector2.Distance(a, b);
                if (!trackingTouches || idA != firstTouchId || idB != secondTouchId)
                {
                    trackingTouches = true;
                    touchDragging = !OverUI(a) && !OverUI(b);
                    firstTouchId = idA;
                    secondTouchId = idB;
                }
                else if (touchDragging)
                {
                    view.Rotate((center - lastCenter) * pixelsToReference, settings);
                    if (lastDistance > 8f && distance > 8f) view.Zoom(lastDistance / distance, settings.ZoomSensitivity);
                    ApplyView();
                }
                lastCenter = center;
                lastDistance = distance;
                return;
            }
            touchDragging = trackingTouches = false;
            if (count > 0) return;
            var mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 position = mouse.position.ReadValue();
            if (mouse.rightButton.wasPressedThisFrame)
            {
                mouseDragging = !OverUI(position);
                lastMouse = position;
            }
            else if (mouse.rightButton.isPressed && mouseDragging)
            {
                view.Rotate((position - lastMouse) * pixelsToReference, settings);
                lastMouse = position;
                ApplyView();
            }
            if (!mouse.rightButton.isPressed) mouseDragging = false;
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f && !OverUI(position))
            {
                view.Zoom(Mathf.Exp(-scroll * 0.0015f), settings.ZoomSensitivity);
                ApplyView();
            }
        }

        static bool OverUI(Vector2 point) => global::Input.TouchInputSystem.IsOverUI(point);

        void ApplyView()
        {
            if (follow != null) follow.FollowOffset = view.Offset(followOffset);
            if (landing != null) landing.FollowOffset = view.Offset(landingOffset);
            if (followCamera != null) followCamera.Lens.OrthographicSize = followSize * view.Distance;
            if (landingCamera != null) landingCamera.Lens.OrthographicSize = landingSize * view.Distance;
        }

        public void ResetView() { view.Reset(); ApplyView(); mouseDragging = touchDragging = trackingTouches = false; }
    }
}
