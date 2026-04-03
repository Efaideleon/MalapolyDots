using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DigitQuadInstantiationSystem : ISystem
    {
        private ComponentLookup<Parent> parentLookup;
        private const int MAX_NUMBER_OF_QUADS = 8;
        private const float QUAD_WIDTH = 0.3f;
        private const float Q_SIGN_OFFSET = 0.2f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<PropertySpaceTag>();

            parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //TODO: probably guard against until the gamescene is loaded, like we do with the buildings...
            parentLookup.Update(ref state);

            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            var prefab = SystemAPI.GetSingleton<QuadEntityPrefab>();

            new SpawnPriceQuadsJob
            {
                QuadPrefab = prefab.Value,
                ParentLookup = parentLookup,
                ecb = ecb,
                QuadWidth = QUAD_WIDTH,
                SignOffset = Q_SIGN_OFFSET,
                MaxQuads = MAX_NUMBER_OF_QUADS
            }.Schedule();
        }

        [BurstCompile]
        [WithNone(typeof(QuadEntitiesBufferProcessed))]
        public partial struct SpawnPriceQuadsJob : IJobEntity
        {
            public Entity QuadPrefab;
            [ReadOnly] public ComponentLookup<Parent> ParentLookup;
            public EntityCommandBuffer ecb;
            public float QuadWidth;
            public float SignOffset;
            public int MaxQuads;

            public void Execute(Entity priceTagEntity, in PriceTagTag _, in SpaceIDComponent spaceId)
            {
                ecb.AddBuffer<QuadsEntitiesBuffer>(priceTagEntity);
                ecb.AddComponent<QuadEntitiesBufferProcessed>(priceTagEntity);
                for (int i = 0; i < MaxQuads; i++)
                {
                    var pricePosition = new float3(-0.8f, 0.2f, -0.09f);
                    var offset = i == 0 ? new float3(-SignOffset, 0, 0) : 0;
                    var quadPos = i * new float3(QuadWidth, 0, 0) + offset + pricePosition;
                    var quadScale = 0.5f;

                    LocalTransform quadTransform = LocalTransform.FromPositionRotationScale(quadPos, quaternion.identity, quadScale);

                    Entity quadEntity = ecb.Instantiate(QuadPrefab);
                    ecb.SetComponent(quadEntity, quadTransform);
                    ecb.AddComponent(quadEntity, new Parent { Value = priceTagEntity });
                    ecb.AddComponent<QuadTag>(quadEntity);
                    ecb.AppendToBuffer(priceTagEntity, new QuadsEntitiesBuffer { Entity = quadEntity });
                }
            }
        }
    }

    public struct QuadTag : IComponentData
    { }

    public struct QuadEntitiesBufferProcessed : IComponentData
    { }
}
