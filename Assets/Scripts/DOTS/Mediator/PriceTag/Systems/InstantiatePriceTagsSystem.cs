using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using DOTS.Mediator;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace a
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InstantiatePriceTagsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PriceTagPivotTag>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();

            if (!gameState.AllPlacesInstantiated)
            {
                UnityEngine.Debug.Log($"[InstantiatePriceTagsSystem] | not all places instantiated.");
                return;
            }

            var priceTagPrefab = SystemAPI.GetSingleton<PriceTagReference>();
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var localToWorld in SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<PriceTagPivotTag>())
            {
                UnityEngine.Debug.Log($"[InstantiatePriceTagsSystem] | pricetagpivot position : {localToWorld.ValueRO.Position}");
                var priceTagInstance = ecb.Instantiate(priceTagPrefab.Entity);
                ecb.SetComponent(priceTagInstance, new LocalTransform
                {
                    Position = localToWorld.ValueRO.Position,
                    Rotation = localToWorld.ValueRO.Rotation,
                    Scale = 1
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Enabled = false;
        }
    }
}
