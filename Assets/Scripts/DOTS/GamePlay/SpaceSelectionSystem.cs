using DOTS.DataComponents;
using DOTS.GameSpaces.Selection;
using Unity.Entities;
using Unity.Rendering;

namespace DOTS.GamePlay
{
    public partial struct SpaceSelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChangeMaterialTag>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<SelectionMaterials>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var propertyClicked in 
                    SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                    >()
                    .WithChangeFilter<LastPropertyClicked>())
            {
                var entityClicked = propertyClicked.ValueRO.entity;
                if (SystemAPI.HasBuffer<LinkedEntityGroup>(entityClicked))
                {
                    UnityEngine.Debug.Log("property has linked entity group clicked");
                    var linkedEntities = SystemAPI.GetBuffer<LinkedEntityGroup>(entityClicked);
                    if (SystemAPI.HasComponent<NameComponent>(propertyClicked.ValueRO.entity))
                    {
                        var nameComp = SystemAPI.GetComponent<NameComponent>(propertyClicked.ValueRO.entity);
                        UnityEngine.Debug.Log($"selected name: {nameComp.Value}");
                    }
                    UnityEngine.Debug.Log($"linked count: {linkedEntities.Length}");
                    foreach (var linkedEntity in linkedEntities)
                    {
                        UnityEngine.Debug.Log("has child entity");
                        Entity childCandidate = linkedEntity.Value;
                        if (childCandidate == entityClicked)
                            continue;

                        if (SystemAPI.HasComponent<ChangeMaterialTag>(childCandidate))
                        {
                            var renderMesh = SystemAPI.GetComponentRW<RenderMeshUnmanaged>(childCandidate);
                            var materials = SystemAPI.ManagedAPI.GetSingleton<SelectionMaterials>();
                            UnityEngine.Debug.Log("Change the material to no selection");
                            renderMesh.ValueRW.materialForSubMesh = materials.NoSelection;
                        }
                    }
                }
            }
        }
    }
}
