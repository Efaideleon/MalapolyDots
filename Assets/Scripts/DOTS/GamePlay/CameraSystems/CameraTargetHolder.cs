using UnityEngine;

namespace DOTS.GamePlay
{
    public class CameraTargetHolder : MonoBehaviour
    {
        public static Transform Instance { get; private set; }

        void Awake()
        {
            Instance = transform;
        }

        void OnDestroy()
        {
            if (Instance == transform)
            {
                Instance = null;
            }
        }
    }
}
