using UnityEngine;
using Unity.Entities;

public class TucTucAuthoring : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] public string Name;

    public class TucTucBaker : Baker<TucTucAuthoring>
    {
        public override void Bake(TucTucAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabComponent { prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)});
            AddComponent(entity, new NameDataComponent { Name = authoring.Name });
        }
    }
}
