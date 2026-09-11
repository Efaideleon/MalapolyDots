using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.EventBuses;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public struct BuyHouseRpc : IRpcCommand
    {
        public int PropertyId;
        public int Count;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RouteBuyHouseToServerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<BuyHouseEventBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<BuyHouseEventBuffer>>())
            {
                foreach (var purchase in buffer)
                {
                    var rpc = ecb.CreateEntity();
                    ecb.AddComponent(rpc, new BuyHouseRpc { PropertyId = purchase.PropertyId, Count = purchase.Count });
                    ecb.AddComponent<SendRpcCommandRequest>(rpc);
                }
                buffer.Clear();
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(MonopolyTrackerSystem))]
    public partial struct BuyHouseSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (purchase, request, rpcEntity) in SystemAPI.Query<RefRO<BuyHouseRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                ecb.DestroyEntity(rpcEntity);
                var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var connection = request.ValueRO.SourceConnection;
                var gameState = SystemAPI.GetSingleton<GameStateComponent>();
                int count = purchase.ValueRO.Count;
                if (count < 1 || count > 4 || !gameState.AllPlacesInstantiated || (gameState.State == GameState.Walking || gameState.State == GameState.GameOver) ||
                    !SystemAPI.HasComponent<NetworkId>(connection) || !SystemAPI.HasComponent<GhostOwner>(player) ||
                    !SystemAPI.HasComponent<GhostMoneyComponet>(player)) continue;
                int buyerId = SystemAPI.GetComponent<NetworkId>(connection).Value;
                if (SystemAPI.GetComponent<GhostOwner>(player).NetworkId != buyerId) continue;

                foreach (var (id, owner, monopoly, houses, price) in SystemAPI.Query<RefRO<SpaceIDComponent>, RefRO<OwnerComponent>,
                    RefRO<MonopolyFlagComponent>, RefRW<HouseCount>, RefRO<HousePriceComponent>>().WithAll<PropertySpaceTag>())
                {
                    if (id.ValueRO.Value != purchase.ValueRO.PropertyId) continue;
                    if (owner.ValueRO.ID != buyerId || !monopoly.ValueRO.Value ||
                        houses.ValueRO.Value < 0 || houses.ValueRO.Value > (houses.ValueRO.Value == 4 ? 5 : 4) - count || price.ValueRO.Value <= 0) break;
                    var money = SystemAPI.GetComponentRW<GhostMoneyComponet>(player);
                    long total = (long)price.ValueRO.Value * count;
                    if (total > money.ValueRO.Value) break;
                    money.ValueRW.Value -= (int)total;
                    houses.ValueRW.Value += count;
                    break;
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
