using UnityEngine;

public class Rotating : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 45f;

    void Update()
    {
        // Rotate around the object's local Y-axis at the given speed
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
