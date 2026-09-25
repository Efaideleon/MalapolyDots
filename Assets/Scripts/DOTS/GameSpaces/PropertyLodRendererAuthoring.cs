using DOTS.DataComponents;
using DOTS.GameSpaces.HouseAuthoring;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    // LOD render entities follow the gameplay state on the original property entity.
    public sealed class PropertyLodRendererAuthoring : MonoBehaviour
    {
        public PropertySpaceAuthoring Property;

        private sealed class Baker : Baker<PropertyLodRendererAuthoring>
        {
            public override void Bake(PropertyLodRendererAuthoring authoring)
            {
                if (authoring.Property == null) return;
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, new PropertyLodSource
                {
                    Value = GetEntity(authoring.Property, TransformUsageFlags.Dynamic)
                });
                AddComponent<MaterialOverrideColorSlider>(entity);
                AddComponent<BlinkingFlagMaterialOverride>(entity);
                AddComponent<HouseColoring1>(entity);
                AddComponent<HouseColoring2>(entity);
                AddComponent<HouseColoring3>(entity);
                AddComponent<HouseColoring4>(entity);
            }
        }
    }

    public struct PropertyLodSource : IComponentData
    {
        public Entity Value;
    }
}
