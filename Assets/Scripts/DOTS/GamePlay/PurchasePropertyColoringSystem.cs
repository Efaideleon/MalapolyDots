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
        private const float coloringSpeed = 20f;
        private const int ColoringThreshold = 100;
        private const int HouseColoringThreshold = 130; // Some building are taller, pick a bigger number.

        private BufferLookup<LinkedEntityGroup> linkedEntitiesBufferLookup; 

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MaterialOverrideColorSlider>();
            state.RequireForUpdate<HouseColoring1>();
            state.RequireForUpdate<HouseColoring2>();
            state.RequireForUpdate<HouseColoring3>();
            state.RequireForUpdate<HouseColoring4>();

            linkedEntitiesBufferLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
        }

        // TODO:
        // This system should only run once, each time the player buys a building.
        // TODO: Currently it is running every frame and resetting the color slider to be equal to threshold.
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            linkedEntitiesBufferLookup.Update(ref state);

            var dt = SystemAPI.Time.DeltaTime;

            DynamicBuffer<LinkedEntityGroup> linkedEntityGroup; 

            foreach (var (tag, houseCounter, entity) in 
                    SystemAPI.Query
                    <
                        RefRO<PropertySpaceTag>,
                        RefRO<HouseCount>
                    >().WithEntityAccess())
            {
                linkedEntityGroup = linkedEntitiesBufferLookup[entity];
                var houseCount = houseCounter.ValueRO.Value;

                for (int i = 0; i < linkedEntityGroup.Length; i++)
                {
                    var currEntity = linkedEntityGroup[i].Value;
                    if (SystemAPI.HasComponent<HouseClusterTag>(currEntity))
                    {
                        if (houseCount > 0)
                        {
                            var house1 = SystemAPI.GetComponentRW<HouseColoring1>(currEntity);
                            var house2 = SystemAPI.GetComponentRW<HouseColoring2>(currEntity);
                            var house3 = SystemAPI.GetComponentRW<HouseColoring3>(currEntity);
                            var house4 = SystemAPI.GetComponentRW<HouseColoring4>(currEntity);

                            if (houseCount == 1)
                            {
                                Color(ref house1.ValueRW.Value, ref dt, HouseColoringThreshold);
                            }
                            if (houseCount == 2)
                            {
                                Color(ref house1.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house2.ValueRW.Value, ref dt, HouseColoringThreshold);
                            }
                            if (houseCount == 3)
                            {
                                Color(ref house1.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house2.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house3.ValueRW.Value, ref dt, HouseColoringThreshold);
                            }
                            if (houseCount == 4) // if you buy 4 houses at once.
                            {
                                Color(ref house1.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house2.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house3.ValueRW.Value, ref dt, HouseColoringThreshold);
                                Color(ref house4.ValueRW.Value, ref dt, HouseColoringThreshold);
                            }
                        }
                    }
                }
            }

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

                    //if  (true)
                    if  (purchased) //BRING BACK
                        Color(ref colorSliderRW.Value, ref dt, ColoringThreshold);
                }
            }
        }
        
        public readonly void Color(ref float value, ref float dt, int threshold)
        {
            if (value < threshold)
            {
                if (true)
                    value = value + coloringSpeed * dt;
            }
            if (value >= threshold)
                value = threshold;
        }
    }
}
