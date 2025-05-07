using Unity.Entities;
using UnityEngine;

public class DebugAuthoring : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    public class DebugBaker : Baker<DebugAuthoring>
    {
        public override void Bake(DebugAuthoring authoring)
        {
            var entity = GetEntity(authoring.prefab, TransformUsageFlags.None);
            AddComponent(entity, new DebugStruct { });
        }
    }
}

public struct DebugStruct : IComponentData
{ }
