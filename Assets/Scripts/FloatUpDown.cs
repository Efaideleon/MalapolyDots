using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    [Header("Movement Settings")]
    public float amplitude = 1f;  // How far it moves up and down
    public float speed = 1f;      // How fast it moves

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Use sine wave for smooth easing in/out motion
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    
    }
}
