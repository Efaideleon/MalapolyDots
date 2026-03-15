using UnityEngine;

namespace DOTS.Utilities
{
    public class DestroyGameObjectOnAwake : MonoBehaviour
    {
        void Awake()
        {
            UnityEngine.Debug.Log($"[DestroyGameObjectOnAwake] | running.");
            Destroy(gameObject);
        }
    }
}
