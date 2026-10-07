using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PropertyLodMaterialSystem))]
    public partial class PropertyBaseGlowSystem : SystemBase
    {
        protected override void OnCreate() => RequireForUpdate<PropertyBaseGlowState>();

        protected override void OnUpdate()
        {
            Dependency.Complete();
            float deltaTime = UnityEngine.Time.unscaledDeltaTime;
            float time = UnityEngine.Time.unscaledTime;
            foreach (var (source, state, animation) in SystemAPI.Query<RefRO<PropertyLodSource>,
                         RefRW<PropertyBaseGlowState>, RefRW<PropertyBaseGlowAnimation>>())
            {
                var property = source.ValueRO.Value;
                if (!EntityManager.HasComponent<MonopolyFlagComponent>(property) ||
                    !EntityManager.HasComponent<HouseCount>(property))
                {
                    state.ValueRW.Value = float4.zero;
                    continue;
                }
                float monopoly = EntityManager.GetComponentData<MonopolyFlagComponent>(property).Value ? 1f : 0f;
                int houses = EntityManager.GetComponentData<HouseCount>(property).Value;
                float flash = animation.ValueRW.Advance(houses, deltaTime);
                state.ValueRW.Value = new float4(monopoly, flash,
                    PropertyBaseGlowAnimation.PurchaseDuration - animation.ValueRO.PurchaseRemaining, time);
            }
        }
    }
}
