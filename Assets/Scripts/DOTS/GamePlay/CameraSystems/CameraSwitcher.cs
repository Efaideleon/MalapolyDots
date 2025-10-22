using UnityEngine;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    public enum Cameras
    {
        Rolling,
        Walking
    }

    public class CameraSwitcher : MonoBehaviour
    {
        [Tooltip("The main camera while rolling.")]
        [SerializeField] Camera? orthographic;

        [Tooltip("The camera while walking.")]
        [SerializeField] Camera? perspective;

        public void OnAwake()
        {
            if (orthographic == null)
            {
                Debug.Log("[CameraSwitcher] | orthographic camera is missing");
            }

            if (perspective == null)
            {
                Debug.Log("[CameraSwitcher] | perspective camera is missing");
            }
        }

        public void SwitchToCamera(Cameras c)
        {
            if (orthographic == null || perspective == null) 
            {
                Debug.Log("[CameraSwitcher] | Cameras are missing");
                return;
            }

            switch(c)
            {
                case Cameras.Walking: 
                    Debug.Log("[CameraSwitcher] | Switching to orthographic camera.");
                    orthographic.enabled = false;
                    perspective.enabled = true;
                    break;

                case Cameras.Rolling:
                    Debug.Log("[CameraSwitcher] | Switching to perspective camera.");
                    orthographic.enabled = false;
                    perspective.enabled = true;
                    break;
            }
        }
        
    }
}
