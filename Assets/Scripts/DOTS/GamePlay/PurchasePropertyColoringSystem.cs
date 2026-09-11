using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.GameSpaces.HouseAuthoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct PurchasePropertyColorSystem : ISystem
    {
        private const float coloringSpeed = 20f;
        private const int ColoringThreshold = 100;
        private const float HouseFadeDuration = 2f;

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
                bool hasLinkedEntities = linkedEntitiesBufferLookup.HasBuffer(entity);
                linkedEntityGroup = hasLinkedEntities ? linkedEntitiesBufferLookup[entity] : default;
                var houseCount = houseCounter.ValueRO.Value;

                int entityCount = hasLinkedEntities ? linkedEntityGroup.Length : 1;
                for (int i = 0; i < entityCount; i++)
                {
                    var currEntity = hasLinkedEntities ? linkedEntityGroup[i].Value : entity;
                    if (SystemAPI.HasComponent<HouseClusterTag>(currEntity))
                    {
                        var house1 = SystemAPI.GetComponentRW<HouseColoring1>(currEntity);
                        var house2 = SystemAPI.GetComponentRW<HouseColoring2>(currEntity);
                        var house3 = SystemAPI.GetComponentRW<HouseColoring3>(currEntity);
                        var house4 = SystemAPI.GetComponentRW<HouseColoring4>(currEntity);

                        FadeHouse(ref house1.ValueRW.Value, houseCount >= 1, dt);
                        FadeHouse(ref house2.ValueRW.Value, houseCount >= 2, dt);
                        FadeHouse(ref house3.ValueRW.Value, houseCount >= 3, dt);
                        FadeHouse(ref house4.ValueRW.Value, houseCount >= 4, dt);
                    }
                }
            }

            // TODO: Re write into a job and only run while coloring.
            foreach (var (materialColorSlider, _) in SystemAPI.Query<RefRW<MaterialOverrideColorSlider>, RefRO<PropertySpaceTag>>())
            {
                var colorSliderRO = materialColorSlider.ValueRO;
                bool coloring = colorSliderRO.Value < ColoringThreshold;

                if (coloring)
                {
                    ref var colorSliderRW = ref materialColorSlider.ValueRW;
                    Color(ref colorSliderRW.Value, ref dt, ColoringThreshold);
                }
            }
        }

        // The shader lerps from its low initial opacity to 1 using these normalized values.
        private static void FadeHouse(ref float value, bool purchased, float deltaTime)
        {
            value = purchased ? math.saturate(value + deltaTime / HouseFadeDuration) : 0f;
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
