using DOTS.Mediator.PanelScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class UIPanelsAuthoring : MonoBehaviour
    {
        public TreasurePanelSO treasurePanelSO;

        public class UIPanelsBaker : Baker<UIPanelsAuthoring>
        {
            public override void Bake(UIPanelsAuthoring authoring)
            {
                var treasurePanelEntity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(treasurePanelEntity, new TreasurePanelData { Panel = authoring.treasurePanelSO });
            }
        }
    }

    public class TreasurePanelData : IComponentData
    {
        public TreasurePanelSO Panel;
    }
}
