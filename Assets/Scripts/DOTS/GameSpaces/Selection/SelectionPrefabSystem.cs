using DOTS.GameSpaces.Selection;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

public partial struct SelectionPrefabSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SelectionPrefabData>();
        state.RequireForUpdate<SelectionMaterialsID>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var selectionMaterialsID = SystemAPI.GetSingleton<SelectionMaterialsID>();

        foreach (var (prefabData, entity) in SystemAPI.Query<SelectionPrefabData>().WithEntityAccess())
        {
            var graphicsSystem = state.World.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            var meshID = graphicsSystem.RegisterMesh(prefabData.mesh);
            ecb.AddComponent(entity, new MaterialMeshInfo
            {
                MaterialID = selectionMaterialsID.NoSelectionMaterialID,
                MeshID = meshID,
                SubMesh = 0
            });
        }
        ecb.Playback(state.EntityManager);
        state.Enabled = false;
    }
}
