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
        public BatchMaterialID materialID;
    }

    [BurstCompile]
    public partial struct SpaceSelectionSystem : ISystem
    {
        private ComponentLookup<ChangeMaterialTag> changeMaterialTags;
        private ComponentLookup<MaterialMeshInfo> materialMeshInfos;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChangeMaterialTag>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<SelectionMaterialsID>();
            changeMaterialTags = state.GetComponentLookup<ChangeMaterialTag>(true); 
            materialMeshInfos = state.GetComponentLookup<MaterialMeshInfo>(true); 
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

                var entityClicked = propertyClicked.ValueRO.entity;
                if (SystemAPI.HasBuffer<LinkedEntityGroup>(entityClicked))
                {
                    var materials = SystemAPI.GetSingleton<SelectionMaterialsID>();
                    var selectionMatID = materials.SelectionMaterialID;

                    var linkedEntities = SystemAPI.GetBuffer<LinkedEntityGroup>(entityClicked);

                    var job = new ChangeMaterialJob
                    {
                        EntityClicked = entityClicked,
                        LinkedEntities = linkedEntities,
                        MaterialID = selectionMatID,
                        ChangeMaterialTags = changeMaterialTags,
                        MaterialMeshInfos = materialMeshInfos,
                        ecbParallel = GetECB(ref state).AsParallelWriter()
                    };

                    var jobHandle = job.Schedule(linkedEntities.Length, 2);
                    state.Dependency = jobHandle;
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
        [ReadOnly] public DynamicBuffer<LinkedEntityGroup> LinkedEntities;
        [ReadOnly] public BatchMaterialID MaterialID;
        [ReadOnly] public ComponentLookup<ChangeMaterialTag> ChangeMaterialTags;
        [ReadOnly] public ComponentLookup<MaterialMeshInfo> MaterialMeshInfos;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            Entity childCandidate = LinkedEntities[index].Value;
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
