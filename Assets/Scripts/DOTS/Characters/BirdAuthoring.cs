using UnityEngine;
using Unity.Entities;

public class BirdAuthoring : MonoBehaviour
{
    [SerializeField] public string Name;

    public class BirdBaker : Baker<BirdAuthoring>
    {
        public override void Bake(BirdAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new BirdDataComponent { Name = authoring.Name});
        }
    }
}
