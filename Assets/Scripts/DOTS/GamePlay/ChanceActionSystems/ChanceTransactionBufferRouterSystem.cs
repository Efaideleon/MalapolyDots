using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.ChanceActionSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ChanceTransactionBufferRouterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<ActivePlayer>();
            state.EntityManager.CreateSingletonBuffer<ChanceBufferEvent>();
            state.RequireForUpdate<ChanceBufferEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<ChanceRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var buffer = SystemAPI.GetSingletonBuffer<ChanceBufferEvent>();
                buffer.Add(new ChanceBufferEvent { });
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ChanceBufferEvent : IBufferElementData
    { }
}
