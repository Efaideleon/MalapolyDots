using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class NumbersMaterialAssetsAuthoring : MonoBehaviour
    {
        public Mesh mesh;
        public Material material;

        public class MaterialAssetsBaker : Baker<NumbersMaterialAssetsAuthoring>
        {
            public override void Bake(NumbersMaterialAssetsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new AssetsMaterial 
                {
                    material = authoring.material,
                    mesh = authoring.mesh
                });
            }
        }
    }

    public class AssetsMaterial : IComponentData
    {
        public Material material;
        public Mesh mesh;
    }
}
