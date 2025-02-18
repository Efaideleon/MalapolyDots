using UnityEngine;
using Unity.Entities;

public class MugAuthoring : MonoBehaviour
{
    [SerializeField] public string Name;

    public class MugBaker : Baker<MugAuthoring>
    {
        public override void Bake(MugAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new MugDataComponent { Name = authoring.Name});
        }
    }
}
