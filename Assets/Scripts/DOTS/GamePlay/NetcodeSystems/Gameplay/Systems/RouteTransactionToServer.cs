using Assets.Scripts.DOTS.Characters;
using DOTS.EventBuses;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RouteTransactionToServer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TransactionEventBuffer>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<BackDropEventBus>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            // TODO: ensure buffer capicty to prevent overloading the server.
            foreach (var transactionBuffer in SystemAPI.Query<DynamicBuffer<TransactionEventBuffer>>().WithChangeFilter<TransactionEventBuffer>())
            {
                foreach (var transaction in transactionBuffer)
                {
                    switch (transaction.EventType)
                    {
                        case TransactionEventType.ChangeTurn:
                            UnityEngine.Debug.Log($"[RouteTransactionToServer] | ChangeTurnRpc Create Entity {state.World}");
                            Entity rpcEntity = ecb.CreateEntity();
                            ecb.AddComponent<ChangeTurnRpc>(rpcEntity);
                            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);

                            foreach (var bus in SystemAPI.Query<DynamicBuffer<BackDropEventBus>>().WithAll<ActivePlayer>())
                            {
                                bus.Add(new BackDropEventBus { });
                            }
                            break;
                        case TransactionEventType.Treasure:
                            // TODO: DO we send an rpc here to close the treasure?
                            // We'll have the server assume that if they receive this rpc what they need to play the close animation.
                            UnityEngine.Debug.Log($"[RouteTransactionToServer] | TreasureRpc Create Entity {state.World}");
                            Entity treasureRpcEntity = ecb.CreateEntity();
                            ecb.AddComponent<TreasureRpc>(treasureRpcEntity);
                            ecb.AddComponent<SendRpcCommandRequest>(treasureRpcEntity);
                            break;
                    }
                }
                transactionBuffer.Clear();
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ChangeTurnRpc : IRpcCommand
    { }

    public struct TreasureRpc : IRpcCommand
    { }
}
