using UnityEngine;
using Unity.Entities;

public class AvocadoAuthoring : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] public string Name;

    public class AvocadoBaker : Baker<AvocadoAuthoring>
    {
        public override void Bake(AvocadoAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabComponent { prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)});
            AddComponent(entity, new NameDataComponent { Name = authoring.Name });
        }
    }
}
