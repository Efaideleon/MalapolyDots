using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class PropertySpaceAuthoring : MonoBehaviour
    {
        public PropertySpaceData Data;

        class PropertySpaceAuthoringBaker : Baker<PropertySpaceAuthoring>
        {
            private const string BlinkPropertyName = "_Blink";

            public override void Bake(PropertySpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new PriceComponent { Value = authoring.Data.price });
                AddComponent(entity, new SpaceTypeComponent { Value = authoring.Data.SpaceType });
                AddComponent(entity, new OwnerComponent { ID = PropertyConstants.Vacant });
                AddComponent(entity, new OwnerByEntityComponent { OwnerEntity = Entity.Null });
                AddComponent(entity, new PropertySpaceTag { });
                AddComponent(entity, new RentComponent { Value = 0 });
                AddComponent(entity, new ColorCodeComponent { Value = authoring.Data.Color });
                AddComponent(entity, new MonopolyFlagComponent { Value = false });
                AddComponent(entity, new HouseCount { Value = 0 });
                AddComponent(entity, new MaterialOverrideColorSlider { Value = 0 });
                AddComponent(entity, new ForSaleComponent { entity = default });
                AddComponent(entity, new BlinkingFlagMaterialOverride { Value = 0f });

                // if (!authoring.TryGetComponent<MeshRenderer>(out var renderer))
                // {
                //     Debug.LogWarning(
                //             @$"[PropertySpaceAuthoring] | 
                //             Entity {authoring.name} is missing a MeshRenderer.
                //             Sikipping material property components");
                // }
                // else
                // {
                //     var material = renderer.sharedMaterial;
                //     if (material != null && material.HasProperty(BlinkPropertyName))
                //     {
                //         AddComponent(entity, new BlinkingFlagMaterialOverride { Value = 0f });
                //     }
                //     else
                //     {
                //         Debug.LogWarning(
                //                 @$"[PropertySpaceAuthoring] | 
                //                 Material {material?.name} on object {authoring.name}
                //                 does not have the {BlinkPropertyName} property defined.");
                //     }
                // }
                //
                var rentBuffer = AddBuffer<BaseRentBuffer>(entity);
                foreach (var rent in authoring.Data.rent)
                {
                    rentBuffer.Add(new BaseRentBuffer { Value = rent });
                }

                // Components for quads
                AddBuffer<QuadDataBuffer>(entity);
                AddBuffer<QuadsEntitiesBuffer>(entity);
            }
        }
    }

    public struct PropertySpaceTag : IComponentData
    { }

    [MaterialProperty("_blink")]
    public struct BlinkingFlagMaterialOverride : IComponentData
    {
        public float Value;
    }

    public struct QuadsEntitiesBuffer : IBufferElementData
    {
        public Entity Entity;
    }
}
