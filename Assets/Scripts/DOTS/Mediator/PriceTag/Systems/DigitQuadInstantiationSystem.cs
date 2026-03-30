using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.InputSystem.iOS;

namespace DOTS.Mediator
{
    //TODO: Theses entities should be instantiated in the client.
    // but the information should be transferred from the server to the client here.


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
            //state.RequireForUpdate<NumberToUVOffset>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<PropertySpaceTag>();
            //state.RequireForUpdate<QuadsEntitiesBuffer>();
            //state.RequireForUpdate<LocalQuadBufferTag>();

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

            // this is the buffer with the quad data
            public void Execute(Entity priceTagEntity, in PriceTagTag _, in LocalTransform localTransform, in SpaceIDComponent spaceId)
            {
                UnityEngine.Debug.Log($"[DigitQuadInstantiationSystem] | instantiating quad entities for spaceID: {spaceId.Value}");
                var buffer = ecb.AddBuffer<QuadsEntitiesBuffer>(priceTagEntity);
                ecb.AddComponent<QuadEntitiesBufferProcessed>(priceTagEntity);
                for (int i = 0; i < MaxQuads; i++)
                {
                    var pricePosition = new float3(-0.8f, 0.2f, -0.09f);
                    var offset = i == 0 ? new float3(-SignOffset, 0, 0) : 0;
                    var quadPos = i * new float3(QuadWidth, 0, 0) + offset + pricePosition;
                    //var quadPos = i * new float3(QuadWidth, 0, 0) + offset + new float3(-83.373f, 2f, -14f);
                    UnityEngine.Debug.Log($"[DigitQuadInstantiationSystem] | spaceid: {spaceId.Value}  spawning quad at: {quadPos}");
                    var quadScale = 0.5f;

                    LocalTransform quadTransform = LocalTransform.FromPositionRotationScale(quadPos, quaternion.identity, quadScale);

                    Entity quadEntity = ecb.Instantiate(QuadPrefab);
                    ecb.SetComponent(quadEntity, quadTransform);
                    ecb.AddComponent(quadEntity, new Parent { Value = priceTagEntity });
                    ecb.AddComponent<QuadTag>(quadEntity);
                    ecb.AppendToBuffer(priceTagEntity, new QuadsEntitiesBuffer { Entity = quadEntity });

                    // UnityEngine.Debug.Log($"[DigitQuadInstantiationSystem] | adding quadEntity {quadEntity}");
                }
            }
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SetupLocalQuadBufferEntity : ISystem
    {
        private ComponentLookup<Parent> parentLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            //state.RequireForUpdate<NumberToUVOffset>();
            state.RequireForUpdate<QuadEntityPrefab>();
            //state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<PriceTagPivotTag>();

            parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            // parentLookup.Update(ref state);
            //
            // var ecb = new EntityCommandBuffer(Allocator.Temp);
            //
            // foreach (var (_, worldTransformPivot, priceTagPivotEntity)
            //         in SystemAPI.Query<RefRO<PriceTagPivotTag>, RefRO<LocalToWorld>>().WithEntityAccess().WithNone<PivotTransformProcessed>())
            // {
            //     if (!parentLookup.HasComponent(priceTagPivotEntity)) continue;
            //     var placeEntity = parentLookup[priceTagPivotEntity];
            //
            //     //UnityEngine.Debug.Log($"[SetupLocalQuadBufferEntity] | worldTransform position: {worldTransform.ValueRO.Position}");
            //     if (SystemAPI.HasComponent<SpaceIDComponent>(placeEntity.Value))
            //     {
            //         UnityEngine.Debug.Log($"[SetupLocalQuadBufferEntity] | loading the quad information");
            //         var spaceID = SystemAPI.GetComponent<SpaceIDComponent>(placeEntity.Value);
            //         //UnityEngine.Debug.Log($"[SetupLocalQuadBufferEntity] | setting up quadBufferEntity for spaceId: {spaceID.Value}");
            //         var pricePivotEntity = ecb.CreateEntity();
            //         ecb.AddComponent<LocalQuadBufferTag>(pricePivotEntity);
            //         ecb.AddBuffer<QuadsEntitiesBuffer>(pricePivotEntity);
            //         ecb.AddComponent(pricePivotEntity, new SpaceIDComponent { Value = spaceID.Value });
            //         ecb.SetName(pricePivotEntity, "SpaceID: " + spaceID.Value.ToString());
            //         ecb.AddComponent(pricePivotEntity, new LocalTransform
            //         {
            //             Position = worldTransformPivot.ValueRO.Position,
            //             Rotation = worldTransformPivot.ValueRO.Rotation,
            //             Scale = 1
            //         });
            //         ecb.AddComponent<PivotTransformProcessed>(priceTagPivotEntity);
            //     }
            // }
            //
            // ecb.Playback(state.EntityManager);
            // ecb.Dispose();
        }
    }

    public struct LocalQuadBufferTag : IComponentData
    { }

    public struct QuadUVs : IComponentData
    {
        public float2 Offset;
    }

    public struct QuadTag : IComponentData
    { }

    public struct PivotTransformProcessed : IComponentData
    { }

    public struct QuadEntitiesBufferProcessed : IComponentData
    { }
}
