using DOTS.GameSpaces.Selection;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Collections;
using UnityEngine.Rendering;

namespace DOTS.GamePlay
{
    public struct LastSelectionChanged : IComponentData
    {
        public Entity entity;
    }

    [BurstCompile]
    public partial struct SpaceSelectionSystem : ISystem
    {
        private ComponentLookup<ChangeMaterialTag> changeMaterialTags;
        private ComponentLookup<MaterialMeshInfo> materialMeshInfos;
        private BufferLookup<LinkedEntityGroup> linkedEntityGroups;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChangeMaterialTag>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<SelectionMaterialsID>();
            state.EntityManager.CreateSingleton<LastSelectionChanged>();
            changeMaterialTags = state.GetComponentLookup<ChangeMaterialTag>(true); 
            materialMeshInfos = state.GetComponentLookup<MaterialMeshInfo>(true); 
            linkedEntityGroups = state.GetBufferLookup<LinkedEntityGroup>(true); 
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var propertyClicked in 
                    SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                    >()
                    .WithChangeFilter<LastPropertyClicked>())
            {
                changeMaterialTags.Update(ref state);
                materialMeshInfos.Update(ref state);
                linkedEntityGroups.Update(ref state);

                var entityClicked = propertyClicked.ValueRO.entity;
                if (SystemAPI.HasBuffer<LinkedEntityGroup>(entityClicked))
                {
                    var materials = SystemAPI.GetSingleton<SelectionMaterialsID>();
                    var selectionMatID = materials.SelectionMaterialID;
                    var noSelectionMatID = materials.NoSelectionMaterialID;

                    var lastSelectionChanged = SystemAPI.GetSingleton<LastSelectionChanged>();
                    JobHandle stateDeps = state.Dependency;
                    JobHandle chainedHandle = stateDeps;
                    if (lastSelectionChanged.entity != entityClicked && lastSelectionChanged.entity != Entity.Null)
                    {
                        var linkedEntities = linkedEntityGroups[lastSelectionChanged.entity];
                        var count = linkedEntities.Length;
                        var job = new ChangeMaterialJob
                        {
                            EntityClicked = lastSelectionChanged.entity,
                            LinkedEntities = linkedEntityGroups,
                            MaterialID = noSelectionMatID,
                            ChangeMaterialTags = changeMaterialTags,
                            MaterialMeshInfos = materialMeshInfos,
                            ecbParallel = GetECB(ref state).AsParallelWriter()
                        };
                        chainedHandle = job.Schedule(count, 2, chainedHandle);
                    }
                    if (entityClicked != Entity.Null)
                    {
                        var linkedEntities = linkedEntityGroups[entityClicked];
                        var count = linkedEntities.Length;
                        var job = new ChangeMaterialJob
                        {
                            EntityClicked = entityClicked,
                            LinkedEntities = linkedEntityGroups,
                            MaterialID = selectionMatID,
                            ChangeMaterialTags = changeMaterialTags,
                            MaterialMeshInfos = materialMeshInfos,
                            ecbParallel = GetECB(ref state).AsParallelWriter()
                        };
                        SystemAPI.GetSingletonRW<LastSelectionChanged>().ValueRW.entity = entityClicked;
                        chainedHandle = job.Schedule(count, 2, chainedHandle);
                    }
                    state.Dependency = chainedHandle;
                }
            }
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }

    [BurstCompile]
    public partial struct ChangeMaterialJob : IJobParallelFor
    {
        public Entity EntityClicked;
        [ReadOnly] public BufferLookup<LinkedEntityGroup> LinkedEntities;
        [ReadOnly] public BatchMaterialID MaterialID;
        [ReadOnly] public ComponentLookup<ChangeMaterialTag> ChangeMaterialTags;
        [ReadOnly] public ComponentLookup<MaterialMeshInfo> MaterialMeshInfos;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            var linkedEntities = LinkedEntities[EntityClicked];
            Entity childCandidate = linkedEntities[index].Value;
            if (childCandidate != EntityClicked)
                if (ChangeMaterialTags.HasComponent(childCandidate))
                {
                    ecbParallel.SetComponent(index, childCandidate, new MaterialMeshInfo 
                    {
                        MaterialID = MaterialID,
                        MeshID = MaterialMeshInfos[childCandidate].MeshID,
                        SubMesh = MaterialMeshInfos[childCandidate].SubMesh
                    });
                }
        }
    }
}
