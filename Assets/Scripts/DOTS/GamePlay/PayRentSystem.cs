using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    [UpdateAfter(typeof(RentCalculatorSystem))]
    public partial struct PayRentSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<OwnerByEntityComponent>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, request, entity) in SystemAPI.Query<RefRO<PayRentRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                ecb.DestroyEntity(entity);
                var connection = request.ValueRO.SourceConnection;
                if (!SystemAPI.HasComponent<GhostOwner>(activePlayerEntity) || !SystemAPI.HasComponent<NetworkId>(connection) ||
                    SystemAPI.GetComponent<GhostOwner>(activePlayerEntity).NetworkId != SystemAPI.GetComponent<NetworkId>(connection).Value ||
                    !SystemAPI.HasComponent<LandingPaymentResolved>(activePlayerEntity) ||
                    SystemAPI.GetComponent<LandingPaymentResolved>(activePlayerEntity).Value ||
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Landing) continue;
                var spaceLandedOnEntity = SystemAPI.GetComponent<SpaceLandedOn>(activePlayerEntity).entity;

                // Did we land on a property.
                if (SystemAPI.HasComponent<PropertySpaceTag>(spaceLandedOnEntity))
                {
                    // Does the property have an owner.
                    var ownerEntity = SystemAPI.GetComponent<OwnerByEntityComponent>(spaceLandedOnEntity).Entity;
                    if (ownerEntity != Entity.Null && ownerEntity != activePlayerEntity && SystemAPI.HasComponent<GhostMoneyComponet>(ownerEntity))
                    {
                        var rent = SystemAPI.GetComponent<GhostRentComponent>(spaceLandedOnEntity).Value;
                        var playerMoney = SystemAPI.GetComponentRW<GhostMoneyComponet>(activePlayerEntity);
                        var ownerMoney = SystemAPI.GetComponentRW<GhostMoneyComponet>(ownerEntity);

                        // Rent transaction.
                        int paid = Unity.Mathematics.math.min(rent, Unity.Mathematics.math.max(0, playerMoney.ValueRO.Value));
                        playerMoney.ValueRW.Value -= rent;
                        ownerMoney.ValueRW.Value += paid;
                        SystemAPI.SetComponent(activePlayerEntity, new LandingPaymentResolved { Value = true });
                    }
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
