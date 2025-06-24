using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelperMethods
{
    public static (float2, RayData) GetClickPositionAndRay()
    {
        var clickPositionVector = Mouse.current.position.ReadValue();
        float2 clickPositionFloat2 = new(clickPositionVector.x, clickPositionVector.y);
        Ray ray = Camera.main.ScreenPointToRay(clickPositionVector);
        RayData rayData = new() { origin = ray.origin, direction = ray.direction };
        return (clickPositionFloat2, rayData);
    }

    public static float2 GetClickPosition(Vector2 position) => new float2(position.x, position.y);

    public static RayData GetRayData(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RayData rayData = new() { origin = ray.origin, direction = ray.direction };
        return rayData;
    }

    public static RayData GetRayBeforeData(Vector3 position, Vector3 deltaPosition)
    {
        Ray rayBefore = Camera.main.ScreenPointToRay(position - deltaPosition);
        RayData rayData = new() { origin = rayBefore.origin, direction = rayBefore.direction };
        return rayData;
    }

    public static void SetClickData(ref ClickData clickData, Vector2 clickPosition, InputActionPhase phase)
    {
        clickData.Position = new float2(clickPosition.x, clickPosition.y);
        clickData.Phase = phase;
    }

    public static void SetRayCastData(ref ClickRayCastData clickRayCastData, RayData rayData, float rayLength)
    {
        clickRayCastData.RayOrigin = rayData.origin;
        clickRayCastData.RayDirection = rayData.direction;
        clickRayCastData.RayEnd = rayData.origin + (rayData.direction * rayLength);
    }

    public static void SetDeltaRayCastData(ref DeltaClickRayCastData clickRayCastData, float rayLenght, RayData rayData)
    {
        clickRayCastData.RayOrigin = rayData.origin;
        clickRayCastData.RayDirection = rayData.direction;
        clickRayCastData.RayEnd = rayData.origin + (rayData.direction * rayLenght);
    }
}
