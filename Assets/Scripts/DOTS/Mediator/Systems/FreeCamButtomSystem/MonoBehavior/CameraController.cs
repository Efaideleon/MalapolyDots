using UnityEngine;
using UnityEngine.UIElements;
using Unity.Collections;
using System.Collections;

namespace Assets.Scripts.DOTS.Mediator.Systems.FreeCamButtomSystem.MonoBehavior
{
    public enum RotationAnimation
    {
        Playing,
        Stopped
    }

    [RequireComponent(typeof(Transform))]
    public class CameraController : MonoBehaviour
    {
        private RotationAnimation RotationAnimation;
        private const float ROTATIONSPEED = 40f;
        private const float MAXROTATION = 90f;

        private Button _rotateCameraButton = null;
        private float _newRotation = 0f;
        private Transform _transform;

        private void Start()
        {
            RotationAnimation = RotationAnimation.Stopped;
            _transform = GetComponent<Transform>();
            Debug.Log($"[CameraController] | LIVES ON: '{gameObject.name}'");
            Debug.Log($"[CameraController] | POSITION: {_transform.position}");
        }

        private void Update()
        {
            if (_rotateCameraButton == null)
            {
                // UnityEngine.Debug.Log($"[CameraController] | trying to find button");
                if (TryGetRotateButton(out _rotateCameraButton))
                {
                    UnityEngine.Debug.Log($"[CameraController] | _rotateCameraButton found!");
                    Subscribe();
                }
            }
        }

        private void Subscribe()
        {
            _rotateCameraButton.clicked += RotateCamera;
        }

        private IEnumerator AnimationCoroutine()
        {
            _newRotation = 0;
            while (_newRotation < MAXROTATION)
            {
                float step = ROTATIONSPEED * Time.deltaTime; // smooth, frame-rate independent
                var transform = GetComponent<Transform>();
                transform.Rotate(0f, step, 0f);
                _newRotation += step;

                Debug.Log($"[CameraController] | rotation camera : {_newRotation}");
                yield return null;
                RotationAnimation = RotationAnimation.Playing;
            }
            RotationAnimation = RotationAnimation.Stopped;
        }

        private void RotateCamera()
        {
            if (RotationAnimation == RotationAnimation.Stopped)
            {
                StartCoroutine(AnimationCoroutine());
            }
        }

        private bool TryGetRotateButton(out Button button)
        {
            button = null;
            var go = GameObject.Find("GameScreenUIGO(Clone)");
            var uiDocument = go?.GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                //Debug.LogWarning("[CameraController] | No UIDocument found in scene!");
                return false;
            }

            var root = uiDocument.rootVisualElement;

            if (root == null)
            {
                //Debug.LogWarning("[CameraController] | rootVisualElement is null — UIDocument not ready yet");
                return false;
            }

            button = root.Q<Button>("RotateCameraButton");
            return button != null;
        }

        private void OnDisable()
        {
            if (_rotateCameraButton != null)
            {
                _rotateCameraButton.clicked -= RotateCamera;
            }
        }
    }
}
