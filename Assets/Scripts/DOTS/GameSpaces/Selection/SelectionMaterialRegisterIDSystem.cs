using Unity.Entities;
using Unity.Rendering;
using UnityEngine.Rendering;

namespace DOTS.GameSpaces.Selection
{
    public partial struct SelectionMaterialRegisterIDSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectionMaterials>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.ManagedAPI.TryGetSingleton<SelectionMaterials>(out var materials))
            {
                var graphicsSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
                BatchMaterialID selectionMatID = graphicsSystem.RegisterMaterial(materials.Selection);
                BatchMaterialID noSelectionMatID = graphicsSystem.RegisterMaterial(materials.NoSelection);
                state.EntityManager.CreateSingleton(new SelectionMaterialsID
                {
                    SelectionMaterialID = selectionMatID,
                    NoSelectionMaterialID = noSelectionMatID
                });
                state.Enabled = false;
            }
        }
    }

    public struct SelectionMaterialsID : IComponentData
    {
        public BatchMaterialID SelectionMaterialID;
        public BatchMaterialID NoSelectionMaterialID;
    }
}
