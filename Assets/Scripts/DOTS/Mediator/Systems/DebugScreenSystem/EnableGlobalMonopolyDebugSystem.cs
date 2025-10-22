using DOTS.DataComponents;
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
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var enabled in SystemAPI.Query<RefRO<GlobalMonopolyEnabled>>().WithChangeFilter<GlobalMonopolyEnabled>())
            {
                if (enabled.ValueRO.Value)
                {
                    foreach (var monopoly in SystemAPI.Query<RefRW<MonopolyFlagComponent>>())
                    {
                        monopoly.ValueRW.Value = true;
                    }
                }
                else
                {
                    foreach (var monopoly in SystemAPI.Query<RefRW<MonopolyFlagComponent>>())
                    {
                        monopoly.ValueRW.Value = false;
                    }
                }
            }
        }
    }
}
