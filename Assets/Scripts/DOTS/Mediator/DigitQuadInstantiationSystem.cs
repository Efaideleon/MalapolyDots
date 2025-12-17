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
    /// This system assumes that PricePivotTransformTag Entity has a parent PriceTagPivotTag Entity that has a parent PropertySpaceTag Entity.
    /// It also assumes that the PropertySpaceTag Entity has a QuadsEntitiesBuffer.
    ///</summary>
    [BurstCompile]
    public partial struct DigitQuadInstantiationSystem : ISystem
    {
        private ComponentLookup<Parent> parentComponentLookup;
        private const int MAX_NUMBER_OF_QUADS = 8;
        private const float QUAD_WIDTH = 0.3f;
        private const float Q_SIGN_OFFSET = 0.2f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NumberToUVOffset>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<PricePivotTransformTag>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<QuadsEntitiesBuffer>();

            parentComponentLookup = SystemAPI.GetComponentLookup<Parent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            parentComponentLookup.Update(ref state);

            var ecb = GetECB(ref state);
            var prefab = SystemAPI.GetSingleton<QuadEntityPrefab>();

            new SpawnPriceQuadsJob
            {
                QuadPrefab = prefab.Value,
                parentComponentLookup = parentComponentLookup,
                ecb = ecb.AsParallelWriter()
            }.ScheduleParallel();

            state.Enabled = false;
        }

        [BurstCompile]
        public partial struct SpawnPriceQuadsJob : IJobEntity
        {
            public Entity QuadPrefab;
            [ReadOnly] public ComponentLookup<Parent> parentComponentLookup;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute([ChunkIndexInQuery] int index, Entity priceTranformEntity, PricePivotTransformTag _)
            {
                if (!parentComponentLookup.HasComponent(priceTranformEntity)) return;
                var parentEntity = parentComponentLookup[priceTranformEntity];

                if (!parentComponentLookup.HasComponent(parentEntity.Value)) return;
                var placeEntity = parentComponentLookup[parentEntity.Value];

                for (int i = 0; i < MAX_NUMBER_OF_QUADS; i++)
                {
                    var offset = i == 0 ? new float3(-Q_SIGN_OFFSET, 0, 0) : 0;
                    var quadPos = i * new float3(QUAD_WIDTH, 0, 0) + offset;
                    var quadScale = 0.5f;

                    LocalTransform quadTransform = LocalTransform.FromPositionRotationScale(quadPos, quaternion.identity, quadScale);

                    Entity quadEntity = ecb.Instantiate(index, QuadPrefab);
                    ecb.SetComponent(index, quadEntity, quadTransform);
                    ecb.AddComponent(index, quadEntity, new Parent { Value = priceTranformEntity });
                    ecb.AppendToBuffer(index, placeEntity.Value, new QuadsEntitiesBuffer { Entity = quadEntity });
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
