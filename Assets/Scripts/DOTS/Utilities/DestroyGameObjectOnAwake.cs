using UnityEngine;

public class DestroyGameObjectOnAwake : MonoBehaviour
{
    void Awake()
    {
        Destroy(gameObject);
    }
}
