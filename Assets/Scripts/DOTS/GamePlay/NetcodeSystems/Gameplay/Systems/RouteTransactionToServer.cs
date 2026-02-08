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
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var transactionBuffer in SystemAPI.Query<DynamicBuffer<TransactionEventBuffer>>().WithChangeFilter<TransactionEventBuffer>())
            {
                if (transactionBuffer.Length < 1)
                    continue;

                foreach (var transaction in transactionBuffer)
                {
                    switch (transaction.EventType)
                    {
                        case TransactionEventType.ChangeTurn:
                            UnityEngine.Debug.Log($"[RouteTransactionToServer] | ChangeTurnRpc Create Entity");
                            var rpcEntity = ecb.CreateEntity();
                            ecb.AddComponent<ChangeTurnRpc>(rpcEntity);
                            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
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
}
