using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace DOTS.Mediator
{
    ///<summary>
    /// This systems Instantiates all the quads into their correct position.
    /// Sets the quads entities as children of the price pivot.
    ///</summary>
    [BurstCompile]
    public partial struct DigitQuadInstantiationSystem : ISystem
    {
        private BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;
        private ComponentLookup<PriceTagPivotTag> priceTagPivotLookup;
        private const int MAX_NUMBER_OF_QUADS = 8;
        private const float QUAD_WIDTH = 0.6f;
        private const float Q_SIGN_OFFSET = 0.5f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AssetsMaterial>();
            state.RequireForUpdate<NumberToUVOffset>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<PriceTagPivotTag>();

            linkedEntityGroupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>();
            priceTagPivotLookup = SystemAPI.GetComponentLookup<PriceTagPivotTag>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            linkedEntityGroupLookup.Update(ref state);
            priceTagPivotLookup.Update(ref state);

            var ecb = GetECB(ref state);
            var prefab = SystemAPI.GetSingleton<QuadEntityPrefab>();

            new SpawnPriceQuadsJob 
            {
                QuadPrefab = prefab.Value,
                linkedEntityGroupLookup = linkedEntityGroupLookup,
                priceTagPivotLookup = priceTagPivotLookup,
                ecb = ecb.AsParallelWriter()
            }.ScheduleParallel();

            state.Enabled = false;
        }

        [BurstCompile]
        public partial struct SpawnPriceQuadsJob : IJobEntity
        {
            public Entity QuadPrefab;
            [ReadOnly] public BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;
            [ReadOnly] public ComponentLookup<PriceTagPivotTag> priceTagPivotLookup;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute([ChunkIndexInQuery] int index, Entity placeEntity, DynamicBuffer<QuadsEntitiesBuffer> _)
            {
                var linkedEntityGroup = linkedEntityGroupLookup[placeEntity];

                foreach (var child in linkedEntityGroup)
                {
                    // There can be multiple price pivots in a place.
                    if (priceTagPivotLookup.HasComponent(child.Value))
                    {
                        var pivotEntity = child.Value;
                        for (int i = 0; i < MAX_NUMBER_OF_QUADS; i++)
                        {
                            var offset = i == 0 ? new float3(-Q_SIGN_OFFSET, 0, 0) : 0;
                            var quadPos = i * new float3(QUAD_WIDTH, 0, 0) + offset;

                            LocalTransform quadTransform = LocalTransform.FromPosition(quadPos);
                            Entity quadEntity = ecb.Instantiate(index, QuadPrefab);

                            ecb.SetComponent(index, quadEntity, quadTransform);
                            ecb.AddComponent(index, quadEntity, new Parent { Value = pivotEntity });
                            ecb.AppendToBuffer(index, placeEntity, new QuadsEntitiesBuffer { Entity = quadEntity });
                        }
                    }
                }
            }
        }

        public EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }

    public struct QuadUVs : IComponentData
    {
        public float2 Offset;
    }

    [MaterialProperty("_UVOffset")]
    public struct UVOffsetOverride : IComponentData
    {
        public float2 Value;
    }

    [MaterialProperty("_UVScale")]
    public struct UVScaleOverride : IComponentData
    {
        public float2 Value;
    }
}
