using Unity.Entities;
using UnityEngine;

public class OwnerShipLabelCanvasReference : IComponentData
{
    public GameObject CanvasGO;
    public GameObject LabelGO;
    public GameObject TextGO;
}

public class OwnerShipLabelCanvasauthoring : MonoBehaviour
{
    [SerializeField] public Canvas Canvas;
    [SerializeField] public GameObject Label;

    public class OwnerCanvasBaker : Baker<OwnerShipLabelCanvasauthoring>
    {
        public override void Bake(OwnerShipLabelCanvasauthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponentObject(entity, new OwnerShipLabelCanvasReference 
            {
                LabelGO = authoring.Label,
                CanvasGO = authoring.Canvas.gameObject,
            });
        }
    }
}
