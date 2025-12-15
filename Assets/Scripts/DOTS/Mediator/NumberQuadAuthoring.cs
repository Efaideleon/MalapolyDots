using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class NumberQuadAuthoring : MonoBehaviour
    {
        public GameObject quadPrefab;

        public class NumberQuadBaker : Baker<NumberQuadAuthoring>
        {
            public override void Bake(NumberQuadAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var quadEntityPrefab = GetEntity(authoring.quadPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new QuadEntityPrefab { Value = quadEntityPrefab});
            }
        }
    }

    public struct QuadEntityPrefab : IComponentData
    {
        public Entity Value;
    }
}
