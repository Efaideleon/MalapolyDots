using UnityEngine;
using Unity.Entities;

public class BirdAuthoring : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] public string Name;

    public class BirdBaker : Baker<BirdAuthoring>
    {
        public override void Bake(BirdAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabComponent { prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)});
            AddComponent(entity, new NameDataComponent { Name = authoring.Name });
        }
    }
}
