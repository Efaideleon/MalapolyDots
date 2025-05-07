using Unity.Entities;
using UnityEngine;

public class ChildDebugAuthoring : MonoBehaviour
{
    public class ChildDebugBaker : Baker<ChildDebugAuthoring>
    {
        public override void Bake(ChildDebugAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new DebugStruct { });
        }
    }
}
