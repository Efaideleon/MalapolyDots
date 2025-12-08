using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.GameSpaces
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

    public struct VisibleStateComponent : IComponentData
    {
        public VisibleState Value;
    }
}
