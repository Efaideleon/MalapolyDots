using UnityEngine;

namespace DOTS.Utilities
{
    public class DestroyGameObjectOnAwake : MonoBehaviour
    {
        void Awake()
        {
            Destroy(gameObject);
        }
    }
}
