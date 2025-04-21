using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct BuyHouseEventBuffer : IBufferElementData
    {
        // TODO: Change to a property Entity
        // The property is the name of the property to buy a house  
        public FixedString64Bytes property; // rename to propertyName
    }

    [BurstCompile]
    public partial struct BuyHouseSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BuyHouseEventBuffer>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<HouseCount>();
            state.RequireForUpdate<PropertySpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<BuyHouseEventBuffer>>().WithChangeFilter<BuyHouseEventBuffer>())
            {
                if (buffer.Length < 1)
                    continue;

                foreach (var buyHouseEvent in buffer)
                {
                    // process the event
                    foreach (var (name, houseCount, isMonopoly, _) in
                            SystemAPI.Query<
                            RefRO<NameComponent>,
                            RefRW<HouseCount>,
                            RefRO<MonopolyFlagComponent>,
                            RefRO<PropertySpaceTag>
                            >())
                    {
                        if (buyHouseEvent.property == name.ValueRO.Value)
                        {
                            if (houseCount.ValueRO.Value < 4 && isMonopoly.ValueRO.Value)
                            {
                                houseCount.ValueRW.Value++;
                            }
                        }
                    }
                }

                buffer.Clear();
            }
        }
    }
}
