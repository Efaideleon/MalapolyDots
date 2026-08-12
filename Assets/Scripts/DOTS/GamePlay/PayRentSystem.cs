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
    public partial struct PayRentSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<OwnerByEntityComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<PayRentRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var spaceLandedOnEntity = SystemAPI.GetComponent<SpaceLandedOn>(activePlayerEntity).entity;

                // Did we land on a property.
                if (SystemAPI.HasComponent<PropertySpaceTag>(spaceLandedOnEntity))
                {
                    // Does the property have an owner.
                    var ownerEntity = SystemAPI.GetComponent<OwnerByEntityComponent>(spaceLandedOnEntity).Entity;
                    if (ownerEntity != Entity.Null)
                    {
                        var rent = SystemAPI.GetComponent<GhostRentComponent>(spaceLandedOnEntity).Value;
                        var playerMoney = SystemAPI.GetComponentRW<GhostMoneyComponet>(activePlayerEntity);
                        var ownerMoney = SystemAPI.GetComponentRW<GhostMoneyComponet>(ownerEntity);

                        // Rent transaction.
                        playerMoney.ValueRW.Value -= rent;
                        ownerMoney.ValueRW.Value += rent;
                    }
                }
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
