using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ForSaleSignAuthoring : MonoBehaviour
    {
        public Animator AnimatorController;

        public class ForSaleSignBaker : Baker<ForSaleSignAuthoring>
        {
            public override void Bake(ForSaleSignAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new ForSaleSignTag { });
                AddComponent(entity, new VisibleStateComponent { Value = VisibleState.Visible });
                AddComponentObject(entity, new AnimatorReference { Animator = authoring.AnimatorController });
            }
        }
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
