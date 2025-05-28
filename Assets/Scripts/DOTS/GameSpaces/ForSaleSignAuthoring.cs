using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ForSaleSignAuthoring : MonoBehaviour
    {
        public class ForSaleSignBaker : Baker<ForSaleSignAuthoring>
        {
            public override void Bake(ForSaleSignAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new ForSaleSignTag { });
                AddComponent(entity, new VisibleStateComponent { Value = VisibleState.Visible });
                AddComponent(entity, new MaterialOverrideFrame { Value = 0 });
            }
        }
    }

    [MaterialProperty("_frame")]
    public struct MaterialOverrideFrame : IComponentData
    {
        public float Value;
    }

    public struct ForSaleSignTag : IComponentData { }

    public enum VisibleState
    {
        Visible,
        Hidden,
    }

    public struct VisibleStateComponent : IComponentData
    {
        public VisibleState Value;
    }
}
