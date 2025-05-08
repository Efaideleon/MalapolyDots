using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces.Selection
{
    public class SelectionAuthoring : MonoBehaviour
    {
        public Mesh mesh;
        public Material selectMaterial;

        public class SelectionBaker : Baker<SelectionAuthoring>
        {
            public override void Bake(SelectionAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new ChangeMaterialTag { });
                AddComponentObject(entity, new SelectionPrefabData
                {
                    mesh = authoring.mesh
                }); 
            }
        }
    }

    public class SelectionPrefabData : IComponentData
    {
        public Mesh mesh;
    }

    public struct ChangeMaterialTag : IComponentData
    { }
}
