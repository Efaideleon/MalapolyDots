using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class PropertiesAuthoring : MonoBehaviour
    {
        public GameObject[] propertiesPrefab;

        public class PropertiesBaker : Baker<PropertiesAuthoring>
        {
            public override void Bake(PropertiesAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var buffer = AddBuffer<PropertiesPrefabBuffer>(entity);
                foreach (var gameObject in authoring.propertiesPrefab)
                {
                    var prefabEntity = GetEntity(gameObject, TransformUsageFlags.Dynamic);
                    buffer.Add(new PropertiesPrefabBuffer { entity = prefabEntity });
                }
            }
        }
    }

    public struct PropertiesPrefabBuffer : IBufferElementData
    {
        public Entity entity;
    }
}
