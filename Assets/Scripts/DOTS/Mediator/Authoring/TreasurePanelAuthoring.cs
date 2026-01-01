using DOTS.Mediator.PanelScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator.Authoring
{
    public class TreasurePanelAuthoring : MonoBehaviour
    {
        public TreasurePanelSO treasurePanelSO;
        public class TreasurePanelBaker : Baker<TreasurePanelAuthoring>
        {
            public override void Bake(TreasurePanelAuthoring authoring)
            {
                var panelEntity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(panelEntity, new TreasurePanelData { Panel = authoring.treasurePanelSO });
                AddComponent(panelEntity, new TreasurePanelTag { });
            }
        }
    }

    public class TreasurePanelData : IComponentData
    {
        public TreasurePanelSO Panel;
    }

    public struct TreasurePanelTag : IComponentData
    { }
}
