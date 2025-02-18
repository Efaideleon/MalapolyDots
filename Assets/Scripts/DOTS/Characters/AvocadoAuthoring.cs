using UnityEngine;
using Unity.Entities;

public class AvocadoAuthoring : MonoBehaviour
{
    [SerializeField] public string Name;

    public class AvocadoBaker : Baker<AvocadoAuthoring>
    {
        public override void Bake(AvocadoAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new AvocadoDataComponent { Name = authoring.Name});
        }
    }
}
