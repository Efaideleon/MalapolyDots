using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem
{
    public class DebugSettingsAuthoring : MonoBehaviour
    {
        public GameObject DebugSettingsGO;

        public class DebugSettingsAuthoringBaker : Baker<DebugSettingsAuthoring>
        {
            public override void Bake(DebugSettingsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new DebugSettingsRef { Entity = GetEntity(authoring.DebugSettingsGO, TransformUsageFlags.None) });
            }
        }
    }

    public struct DebugSettingsRef : IComponentData
    {
        public Entity Entity;
    }
}
