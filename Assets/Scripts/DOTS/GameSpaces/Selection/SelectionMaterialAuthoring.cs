using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces.Selection
{
    public class SelectionMaterialAuthoring : MonoBehaviour
    {
        [SerializeField] Material Selection;
        [SerializeField] Material NoSelection;

        public class SelectionMaterialBaker : Baker<SelectionMaterialAuthoring>
        {
            public override void Bake(SelectionMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new SelectionMaterials 
                {
                    Selection = authoring.Selection,
                    NoSelection = authoring.NoSelection
                });
            }
        }
    }

    public class SelectionMaterials : IComponentData
    {
        public Material Selection;
        public Material NoSelection;
    }
}
