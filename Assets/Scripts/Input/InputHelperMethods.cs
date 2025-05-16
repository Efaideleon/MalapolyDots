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

    public static (float2, RayData) GetClickPositionAndRay(Vector3 position)
    {
        var clickPositionVector = position;
        float2 clickPositionFloat2 = new(clickPositionVector.x, clickPositionVector.y);
        Ray ray = Camera.main.ScreenPointToRay(clickPositionVector);
        RayData rayData = new() { origin = ray.origin, direction = ray.direction };
        return (clickPositionFloat2, rayData);
    }

    public static void SetClickData(
            ref ClickRayCastData clickRayCastData,
            ref ClickData clickData,
            InputActionPhase phase,
            float rayLenght,
            float2 clickPosition,
            RayData rayData)
    {
        clickRayCastData.RayOrigin = rayData.origin;
        clickRayCastData.RayDirection = rayData.direction;
        clickRayCastData.RayEnd = rayData.origin + (rayData.direction * rayLenght);
        clickData.Position = clickPosition;  
        clickData.Phase = phase;  
    }
}
