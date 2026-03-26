using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class NumberQuadAuthoring : MonoBehaviour
    {
        //TODO: this gameobject has a ghost.
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

    // TODO: this has to be ghost to be instantiated in the server and client.
    // Make sure quad has a ghost componenet.
    public struct QuadEntityPrefab : IComponentData
    {
        public Entity Value;
    }
}
