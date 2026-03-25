using DOTS.Mediator.PanelScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator.Authoring
{
    public class PurchasePanelAuthoring : MonoBehaviour
    {
        public PurchasePropertyPanelSO purchasePropertyPanelSO;
        public class PurchasePanelBaker : Baker<PurchasePanelAuthoring>
        {
            public override void Bake(PurchasePanelAuthoring authoring)
            {
                var panelEntity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(panelEntity, new PurchasePropertyPanelData { Panel = authoring.purchasePropertyPanelSO });
                AddComponent(panelEntity, new PurchasePropertyPanelTag { });
            }
        }
    }
    public struct PurchasePropertyPanelTag : IComponentData
    { }

    public class PurchasePropertyPanelData : IComponentData
    {
        public PurchasePropertyPanelSO Panel;
    }
}
