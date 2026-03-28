using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.Mediator;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace a
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InstantiatePriceTagsSystem : ISystem
    {
        private ComponentLookup<Parent> parentLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<GameStateComponent>();
            parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();

            parentLookup.Update(ref state);
            if (!gameState.AllPlacesInstantiated)
            {
                UnityEngine.Debug.Log($"[InstantiatePriceTagsSystem] | not all places instantiated.");
                return;
            }

            var priceTagPrefab = SystemAPI.GetSingleton<PriceTagReference>();
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (localToWorld, priceTagPivotEntity) in SystemAPI.Query<RefRO<LocalToWorld>>().WithEntityAccess().WithAll<PriceTagPivotTag>())
            {
                if (!parentLookup.HasComponent(priceTagPivotEntity)) continue;
                var placeEntity = parentLookup[priceTagPivotEntity];

                //UnityEngine.Debug.Log($"[SetupLocalQuadBufferEntity] | worldTransform position: {worldTransform.ValueRO.Position}");
                if (SystemAPI.HasComponent<SpaceIDComponent>(placeEntity.Value))
                {
                    UnityEngine.Debug.Log($"[InstantiatePriceTagsSystem] | pricetagpivot position : {localToWorld.ValueRO.Position}");
                    var spaceID = SystemAPI.GetComponent<SpaceIDComponent>(placeEntity.Value);
                    var priceTagInstance = ecb.Instantiate(priceTagPrefab.Entity);
                    ecb.SetComponent(priceTagInstance, new LocalTransform
                    {
                        Position = localToWorld.ValueRO.Position,
                        Rotation = localToWorld.ValueRO.Rotation,
                        Scale = 1
                    });
                    ecb.SetComponent(priceTagInstance, new SpaceIDComponent { Value = spaceID.Value });
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Enabled = false;
        }
    }
}
