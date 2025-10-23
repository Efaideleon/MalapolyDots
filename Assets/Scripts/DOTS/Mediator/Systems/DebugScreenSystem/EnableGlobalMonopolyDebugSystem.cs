using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;

#nullable enable

namespace DOTS.Mediator.Systems.DebugScreenSystem
{
    public struct GlobalMonopolyEnabled : IComponentData
    {
        public bool Value;
    }

    public partial struct EnableGlobalMonopolyDebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new GlobalMonopolyEnabled { Value = false });
            state.RequireForUpdate<GlobalMonopolyEnabled>();
            state.RequireForUpdate<CurrentPlayerID>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var enabled in SystemAPI.Query<RefRO<GlobalMonopolyEnabled>>().WithChangeFilter<GlobalMonopolyEnabled>())
            {
                if (enabled.ValueRO.Value)
                {
                    foreach (var (monopoly, owner) in SystemAPI.Query<RefRW<MonopolyFlagComponent>, RefRW<OwnerComponent>>())
                    {
                        owner.ValueRW.ID = SystemAPI.GetSingleton<CurrentPlayerID>().Value;
                        monopoly.ValueRW.Value = true;
                    }
                }
                else
                {
                    foreach (var (monopoly, owner) in SystemAPI.Query<RefRW<MonopolyFlagComponent>, RefRW<OwnerComponent>>())
                    {
                        owner.ValueRW.ID = PropertyConstants.Vacant;
                        monopoly.ValueRW.Value = false;
                    }
                }
            }
        }
    }
}
