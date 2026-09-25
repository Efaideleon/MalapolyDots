using UnityEngine;

namespace DOTS.GamePlay
{
    public class CameraTargetHolder : MonoBehaviour
    {
        public static Transform Instance { get; private set; }

        void Awake()
        {
            Instance = transform;
            // The scene's saved placeholder may be below the board before ghosts arrive.
            var position = transform.position;
            position.y = Mathf.Max(0f, position.y);
            transform.position = position;
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
