using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class PlacesAuthoring : MonoBehaviour
    {
        public GameObject[] propertiesPrefab;

        public class PlacesBaker : Baker<PlacesAuthoring>
        {
            public override void Bake(PlacesAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var buffer = AddBuffer<PlacesPrefabBuffer>(entity);
                for (int i = 0; i < authoring.propertiesPrefab.Length; i++)
                {
                    var gameObject = authoring.propertiesPrefab[i];
                    var prefabEntity = GetEntity(gameObject, TransformUsageFlags.Dynamic);
                    buffer.Add(new PlacesPrefabBuffer { entity = prefabEntity, BoardIndex = i });
                }
            }
        }
    }

    public struct PlacesPrefabBuffer : IBufferElementData
    {
        public Entity entity;
        public int BoardIndex;
    }
}
