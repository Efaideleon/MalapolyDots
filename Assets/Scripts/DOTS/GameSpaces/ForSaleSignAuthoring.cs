using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Malapoly.DOTS.GameSpaces
{
    public class ForSaleSignAuthoring : MonoBehaviour
    {
        public class ForSaleSignBaker : Baker<ForSaleSignAuthoring>
        {
            public override void Bake(ForSaleSignAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.NonUniformScale);
                AddComponent(entity, new ForSaleSignTag { });
                AddComponent(entity, new VisibleStateComponent { Value = VisibleState.Visible });
            }
        }
    }

    public struct ForSaleSignTag : IComponentData { }

    public enum VisibleState
    {
        Visible,
        Hiding,
        Hidden,
    }

    [GhostComponent(SendDataForChildEntity = true)]
    public struct VisibleStateComponent : IComponentData
    {
        [GhostField]
        public VisibleState Value;
    }
}
