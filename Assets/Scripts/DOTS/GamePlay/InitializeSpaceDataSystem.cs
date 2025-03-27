using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct InitializePropertiesJob : IJobEntity
{
    public BlobAssetReference<PropertiesDataBlob> propertiesReference; 

    public void Execute(
            in PropertySpaceTag _,
            in NameComponent name,
            ref SpaceIDComponent id,
            ref BoardIndexComponent boardIdx,
            ref SpacePriceComponent price,
            ref RentComponent rent
            )
    {
        var numOfProperties = propertiesReference.Value.properties.Length;
        for (int i = 0; i < numOfProperties; i++)
        {
            ref var property = ref propertiesReference.Value.properties[i];
            if (name.Value == property.name)
            {
                id.Value = property.id;
                boardIdx.Value = property.boardIndex;
                price.Value = property.price;
                rent.Value = property.rent;
            }
        }
    }
}

[BurstCompile]
public partial struct InitializeSpaceDataSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    {
        state.RequireForUpdate<PropertiesDataBlobReference>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) 
    {
        state.Enabled = false;
        var propertiesReference = SystemAPI.GetSingleton<PropertiesDataBlobReference>().propertiesBlobReference;

        new InitializePropertiesJob
        {
            propertiesReference = propertiesReference
        }.ScheduleParallel();
    }
}
