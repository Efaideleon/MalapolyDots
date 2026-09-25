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
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct InstantiatePriceTagsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PriceTagReference>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();

            if (!gameState.AllPlacesInstantiated)
            {
                return;
            }

            var priceTagPrefab = SystemAPI.GetSingleton<PriceTagReference>();
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (localToWorld, property, priceTagPivotEntity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<PriceTagProperty>>()
                         .WithEntityAccess().WithAll<PriceTagPivotTag>().WithNone<PriceTagSpawned>())
            {
                var placeEntity = property.ValueRO.Value;

                if (SystemAPI.HasComponent<SpaceIDComponent>(placeEntity))
                {
                    var spaceID = SystemAPI.GetComponent<SpaceIDComponent>(placeEntity);
                    var priceTagInstance = ecb.Instantiate(priceTagPrefab.Entity);
                    ecb.SetComponent(priceTagInstance, new LocalTransform
                    {
                        Position = localToWorld.ValueRO.Position,
                        Rotation = localToWorld.ValueRO.Rotation,
                        Scale = 1
                    });
                    ecb.SetComponent(priceTagInstance, new SpaceIDComponent { Value = spaceID.Value });
                    ecb.AddComponent<PriceTagSpawned>(priceTagPivotEntity);
                    UnityEngine.Debug.Log($"[PriceTag] Created tag for property ID {spaceID.Value} at {localToWorld.ValueRO.Position}.");
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            // Client ghosts may arrive after the server's AllPlacesInstantiated flag.
            // Keep accepting new pivots; the marker prevents duplicate tags.
        }
    }
}
