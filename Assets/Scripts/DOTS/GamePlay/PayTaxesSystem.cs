using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PayTaxesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, request, entity) in SystemAPI.Query<RefRO<PayTaxesRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
                var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var connection = request.ValueRO.SourceConnection;
                if (SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Landing ||
                    !SystemAPI.HasComponent<GhostOwner>(player) || !SystemAPI.HasComponent<NetworkId>(connection) ||
                    SystemAPI.GetComponent<GhostOwner>(player).NetworkId != SystemAPI.GetComponent<NetworkId>(connection).Value ||
                    !SystemAPI.HasComponent<LandingPaymentResolved>(player) ||
                    SystemAPI.GetComponent<LandingPaymentResolved>(player).Value) continue;
                var space = SystemAPI.GetComponent<SpaceLandedOn>(player).entity;
                if (!SystemAPI.HasComponent<TaxAmountComponent>(space)) continue;
                SystemAPI.GetComponentRW<GhostMoneyComponet>(player).ValueRW.Value -= SystemAPI.GetComponent<TaxAmountComponent>(space).Value;
                SystemAPI.SetComponent(player, new LandingPaymentResolved { Value = true });
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
