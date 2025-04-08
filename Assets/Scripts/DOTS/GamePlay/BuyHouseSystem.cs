using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

// The property is the name of the property to buy a house  
public struct BuyHouseEvent : IBufferElementData
{
    public FixedString64Bytes property; // rename to propertyName
}

[BurstCompile]
public partial struct BuyHouseSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddBuffer<BuyHouseEvent>(entity);
        state.RequireForUpdate<BuyHouseEvent>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<HouseCount>();
        state.RequireForUpdate<PropertySpaceTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query< DynamicBuffer<BuyHouseEvent>>().WithChangeFilter<BuyHouseEvent>()) 
        {
            if (buffer.Length < 1)
                return;

            foreach (var buyHouseEvent in buffer)
            {
                // process the event
                // check if a house can be bought. if yes inscrease the number of houses in the property
                foreach (var (name, houseCount, _) in SystemAPI.Query<RefRO<NameComponent>, RefRW<HouseCount>, RefRO<PropertySpaceTag>>())
                {
                    if (buyHouseEvent.property == name.ValueRO.Value)
                    {
                        if ((houseCount.ValueRO.Value + 1) < 4)
                        {
                            houseCount.ValueRW.Value++;
                        }
                    }
                }
            }

            // this may cause an error as an element is being changed inside the foreach loop
            buffer.Clear();
        }
    }
}
