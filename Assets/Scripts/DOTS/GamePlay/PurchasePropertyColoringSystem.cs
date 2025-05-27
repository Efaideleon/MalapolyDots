using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct PurchasePropertyColorSystem : ISystem
    {
        private const float coloringSpeed = 2f;
        private const int ColoringThreshold = 10;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MaterialOverrideColorSlider>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (materialColorSlider, owner, _) in 
                    SystemAPI.Query<
                        RefRW<MaterialOverrideColorSlider>,
                        RefRO<OwnerComponent>,
                        RefRO<PropertySpaceTag>
                    >()) //TODO: Add WithChangeFilter<OwnerComponent>?
            {
                var colorSliderRO = materialColorSlider.ValueRO;
                bool coloring = colorSliderRO.Value < ColoringThreshold;

                if (coloring)
                {
                    var ownerID = owner.ValueRO.ID;
                    ref var colorSliderRW = ref materialColorSlider.ValueRW;

                    bool purchased = ownerID != PropertyConstants.Vacant;

                    if  (purchased)
                        colorSliderRW.Value = colorSliderRO.Value + coloringSpeed * dt;

                    if (colorSliderRO.Value >= ColoringThreshold)
                        colorSliderRW.Value = ColoringThreshold;
                }
            }
        }
    }
}
