using Assets.Scripts.DOTS.Characters;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.EventBuses;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PurchasePropertySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostMoneyComponet>();
            state.RequireForUpdate<SpaceLandedOn>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (rpc, propertyToPurchase, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<PurchasePropertyEventRpc>>().WithEntityAccess())
            {
                var currentPlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                if (currentPlayerEntity != Entity.Null)
                {
                    var playerId = SystemAPI.GetComponent<GhostOwner>(currentPlayerEntity).NetworkId;
                    var clientId = SystemAPI.GetComponent<NetworkId>(rpc.ValueRO.SourceConnection).Value;
                    bool isLocalClient = playerId == clientId;
                    if (isLocalClient)
                    {
                        var landOnProperty = SystemAPI.GetComponent<SpaceLandedOn>(currentPlayerEntity);
                        UnityEngine.Debug.Log($"[PurchasePropertySystem] | landOnProperty {landOnProperty.entity}");
                        if (SystemAPI.HasComponent<SpaceIDComponent>(landOnProperty.entity))
                        {
                            var landedOnID = SystemAPI.GetComponent<SpaceIDComponent>(landOnProperty.entity).Value;

                            UnityEngine.Debug.Log($"[PurchasePropertySystem] | landedOnID: {landedOnID} and propertyToPurchaseID {propertyToPurchase.ValueRO.ID}");
                            bool isPlayerReadyToPurchase = propertyToPurchase.ValueRO.ID == landedOnID;
                            if (isPlayerReadyToPurchase)
                            {
                                // Make sure that the property doesn't already have an owner.
                                var owner = SystemAPI.GetComponentRW<OwnerComponent>(landOnProperty.entity);
                                var ownedByEntity = SystemAPI.GetComponentRW<OwnerByEntityComponent>(landOnProperty.entity);

                                bool isPropertyVacant = owner.ValueRO.ID == PropertyConstants.Vacant;
                                if (isPropertyVacant)
                                {
                                    var propertyPrice = SystemAPI.GetComponent<PriceComponent>(landOnProperty.entity);
                                    ref var playerMoney = ref SystemAPI.GetComponentRW<GhostMoneyComponet>(currentPlayerEntity).ValueRW;
                                    UnityEngine.Debug.Log($"[PurchasePropertySystem] | Property to buy price: {propertyPrice.Value}");
                                    playerMoney.Value -= propertyPrice.Value;
                                    owner.ValueRW.ID = playerId;
                                    ownedByEntity.ValueRW.Entity = currentPlayerEntity;

                                    var name = SystemAPI.GetComponent<NameComponent>(landOnProperty.entity);
                                    UnityEngine.Debug.Log($"[PurchasePropertySystem] | Property Bought! {name.Value}");
                                    var updatedMoney = SystemAPI.GetComponent<GhostMoneyComponet>(currentPlayerEntity);
                                    UnityEngine.Debug.Log($"[PurchasePropertySystem] | player new money {updatedMoney.Value}");
                                }
                                else
                                {
                                    UnityEngine.Debug.Log($"[PurchasePropertySystem] | Property Not For Sale.");
                                }
                            }
                        }

                    }
                }
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RoutePurchasePropertyToServer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollEventBuffer>();
            state.RequireForUpdate<LastPropertyInteracted>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<PurchasePropertyEventBuffer>>().WithChangeFilter<PurchasePropertyEventBuffer>())
            {
                if (buffer.Length < 1)
                    continue;

                foreach (var _ in buffer)
                {
                    var property = SystemAPI.GetSingleton<LastPropertyInteracted>();
                    var propertyID = SystemAPI.GetComponent<SpaceIDComponent>(property.entity);

                    var entity = ecb.CreateEntity();
                    ecb.AddComponent<PurchasePropertyEventRpc>(entity);
                    ecb.SetComponent(entity, new PurchasePropertyEventRpc { ID = propertyID.Value });
                    ecb.AddComponent<SendRpcCommandRequest>(entity);
                }

                buffer.Clear();
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

        }
    }

    public struct PurchasePropertyEventRpc : IRpcCommand
    {
        public int ID;
    }
}
