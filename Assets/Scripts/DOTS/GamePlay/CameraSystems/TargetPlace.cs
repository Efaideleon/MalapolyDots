using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class TargetPlace : MonoBehaviour
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
