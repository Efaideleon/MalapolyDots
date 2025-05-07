using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces.Selection
{
    public class SelectionAuthoring : MonoBehaviour
    {
        public class SelectionBaker : Baker<SelectionAuthoring>
        {
            public override void Bake(SelectionAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Renderable);
                AddComponent(entity, new ChangeMaterialTag { });
            }
        }
    }

    public struct ChangeMaterialTag : IComponentData
    { }
}
