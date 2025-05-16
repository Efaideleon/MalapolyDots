using System.Collections.Generic;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using UnityObject = UnityEngine.Object;

public class LabelGOsComponent : IComponentData
{
    public Dictionary<FixedString64Bytes, GameObject> GameObjects;
}

public class CanvasGORef : IComponentData
{
    public GameObject CanvasGO;
}

public partial struct OwnerShipLabelCanvasSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<OwnerShipLabelCanvasReference>();
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<LabelGOsComponent>();
        state.RequireForUpdate<CanvasGORef>();
        state.EntityManager.CreateSingleton(new LabelGOsComponent { GameObjects = new() });
        state.EntityManager.CreateSingleton(new CanvasGORef { CanvasGO = null });
    }

    public void OnStartRunning(ref SystemState state)
    {
        var canvasRef = SystemAPI.ManagedAPI.GetSingleton<OwnerShipLabelCanvasReference>();
        if (canvasRef == null)
            return;

        if (canvasRef.CanvasGO == null || canvasRef.LabelGO == null)
            return;

        var canvasGO = UnityObject.Instantiate(canvasRef.CanvasGO);
        SystemAPI.ManagedAPI.GetSingleton<CanvasGORef>().CanvasGO = canvasGO;

        var labelGOsRef = SystemAPI.ManagedAPI.GetSingleton<LabelGOsComponent>(); 
        foreach (var (name, _) in SystemAPI.Query<RefRO<NameComponent>, RefRO<PropertySpaceTag>>())
        {
            var labelGO = UnityObject.Instantiate(canvasRef.LabelGO, canvasGO.transform);
            labelGO.SetActive(false);
            labelGOsRef.GameObjects.Add(name.ValueRO.Value, labelGO);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var canvasGO = SystemAPI.ManagedAPI.GetSingleton<CanvasGORef>();
        if (canvasGO.CanvasGO != null)
        {
            var labelGOs = SystemAPI.ManagedAPI.GetSingleton<LabelGOsComponent>().GameObjects;
            var canvas = canvasGO.CanvasGO.GetComponent<Canvas>();
            foreach (var (name, transfrom, _) in SystemAPI.Query<RefRO<NameComponent>, RefRO<LocalTransform>, RefRO<PropertySpaceTag>>())
            {
                RectTransform canvasRect = canvasGO.CanvasGO.GetComponent<RectTransform>();
                RectTransform labelRect = labelGOs[name.ValueRO.Value].GetComponent<RectTransform>();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect, Camera.main.WorldToScreenPoint(transfrom.ValueRO.Position),
                        canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                        out var localPoint);
                var currentLocalPoint = localPoint;
                labelRect.anchoredPosition = new Vector2(currentLocalPoint.x, currentLocalPoint.y + 30);
                // labelRect.anchoredPosition = localPoint; 
            }
            //state.Enabled = true; // TODO: enable when the camera follows the player
        }
    }

    public void OnStopRunning(ref SystemState state)
    { }
}
