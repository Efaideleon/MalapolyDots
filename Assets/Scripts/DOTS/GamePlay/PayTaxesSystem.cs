using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator;
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
            state.RequireForUpdate<TaxSpaceTag>();
            state.RequireForUpdate<SpaceLandedOn>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<PayTaxesRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                UnityEngine.Debug.Log($"[PayTaxesSystem] | Receving pay taxes rpc.");
                foreach (var (playerMoney, spaceLandedOn) in 
                        SystemAPI.Query<
                            RefRW<GhostMoneyComponet>,
                            RefRO<SpaceLandedOn>
                        >()
                        .WithAll<GhostOwnerIsLocal, ActivePlayer>())
                {
                        Entity space = spaceLandedOn.ValueRO.entity;
                        if (space != Entity.Null && SystemAPI.HasComponent<TaxSpaceTag>(space))
                        {
                            // TODO: this value should come from a component in the tax
                            UnityEngine.Debug.Log($"[PayTaxesSystem] | paying taxes in server.");
                            var tax = 100_000;
                            // This is not a ghost component.
                            playerMoney.ValueRW.Value -= tax;
                        }
                }
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
