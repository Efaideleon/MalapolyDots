using DOTS.Mediator.PanelScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class UIPanelsAuthoring : MonoBehaviour
    {
        public TreasurePanelSO treasurePanelSO;
        public PurchasePropertyPanelSO purchasePropertyPanelSO;

        public class UIPanelsBaker : Baker<UIPanelsAuthoring>
        {
            public override void Bake(UIPanelsAuthoring authoring)
            {
                var panelEntity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(panelEntity, new TreasurePanelData { Panel = authoring.treasurePanelSO });
                AddComponentObject(panelEntity, new PurchasePropertyPanelData { Panel = authoring.purchasePropertyPanelSO });
            }
        }
    }

    public class TreasurePanelData : IComponentData
    {
        public TreasurePanelSO Panel;
    }

    public class PurchasePropertyPanelData : IComponentData
    {
        public PurchasePropertyPanelSO Panel;
    }
}
