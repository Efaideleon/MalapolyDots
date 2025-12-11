using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class MaterialAssetsAuthoring : MonoBehaviour
    {
        public Mesh mesh;
        public Material material;

        public class MaterialAssetsBaker : Baker<MaterialAssetsAuthoring>
        {
            public override void Bake(MaterialAssetsAuthoring authoring)
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
