using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.Utilities.ChancesBlob;
using DOTS.Utilities.GoBlob;
using DOTS.Utilities.GoToJailBlob;
using DOTS.Utilities.JailBlob;
using DOTS.Utilities.ParkingBlob;
using DOTS.Utilities.PropertiesBlob;
using DOTS.Utilities.TaxesBlob;
using DOTS.Utilities.TreasuresBlob;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct InitializePropertiesJob : IJobEntity
    {
        public BlobAssetReference<PropertiesDataBlob> propertiesReference;

        public void Execute(
                in PropertySpaceTag _,
                in NameComponent name,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx,
                ref PriceComponent price,
                ref RentComponent rent,
                ref ColorCodeComponent color,
                ref DynamicBuffer<BaseRentBuffer> rentBuffer
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
                    rent.Value = 0;
                    color.Value = property.color;
                    for (int j = 0; j < property.rent.Length; j++)
                    {
                        rentBuffer.Add(new BaseRentBuffer
                                {
                                Value = property.rent[j]
                                });
                    }
                }
            }
        }
    }

    [BurstCompile]
    public partial struct InitializeTreasuresJob : IJobEntity
    {
        public BlobAssetReference<TreasureDataBlob> treasuresReference;

        public void Execute(
                in TreasureSpaceTag _,
                in NameComponent name,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            var numOfTreasures = treasuresReference.Value.treasures.Length;
            for (int i = 0; i < numOfTreasures; i++)
            {
                ref var treasure = ref treasuresReference.Value.treasures[i];
                if (name.Value == treasure.Name)
                {
                    id.Value = treasure.id;
                    boardIdx.Value = treasure.boardIndex;
                }
            }
        }
    }
    [BurstCompile]
    public partial struct InitializeGoToJailJob : IJobEntity
    {
        public BlobAssetReference<GoToJailDataBlob> goToJailReference;

        public void Execute(
                in GoToJailTag _,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            ref var goToJail = ref goToJailReference.Value.goToJail;
            id.Value = goToJail.id;
            boardIdx.Value = goToJail.boardIndex;
        }
    }
    [BurstCompile]
    public partial struct InitializeChancesJob : IJobEntity
    {
        public BlobAssetReference<ChancesDataBlob> chancesReference;

        public void Execute(
                in ChanceSpaceTag _,
                in NameComponent name,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            var numOfChances = chancesReference.Value.chances.Length;
            for (int i = 0; i < numOfChances; i++)
            {
                ref var chance = ref chancesReference.Value.chances[i];
                if (name.Value == chance.Name)
                {
                    id.Value = chance.id;
                    boardIdx.Value = chance.boardIndex;
                }
            }
        }
    }
    [BurstCompile]
    public partial struct InitializeParkingJob : IJobEntity
    {
        public BlobAssetReference<ParkingDataBlob> parkingReference;

        public void Execute(
                in ParkingSpaceTag _,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            ref var parking = ref parkingReference.Value.parking;
            id.Value = parking.id;
            boardIdx.Value = parking.boardIndex;
        }
    }
    [BurstCompile]
    public partial struct InitializeTaxesJob : IJobEntity
    {
        public BlobAssetReference<TaxesDataBlob> taxesReference;

        public void Execute(
                in TaxSpaceTag _,
                in NameComponent name,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            var numOfTaxes = taxesReference.Value.taxes.Length;
            for (int i = 0; i < numOfTaxes; i++)
            {
                ref var tax = ref taxesReference.Value.taxes[i];
                if (name.Value == tax.Name)
                {
                    id.Value = tax.id;
                    boardIdx.Value = tax.boardIndex;
                }
            }
        }
    }
    [BurstCompile]
    public partial struct InitializeJailJob : IJobEntity
    {
        public BlobAssetReference<JailDataBlob> jailReference;

        public void Execute(
                in JailSpaceTag _,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            ref var jail = ref jailReference.Value.jail;
            id.Value = jail.id;
            boardIdx.Value = jail.boardIndex;
        }
    }
    [BurstCompile]
    public partial struct InitializeGoJob : IJobEntity
    {
        public BlobAssetReference<GoDataBlob> goReference;

        public void Execute(
                in GoSpaceTag _,
                ref SpaceIDComponent id,
                ref BoardIndexComponent boardIdx
                )
        {
            ref var go = ref goReference.Value.go;
            id.Value = go.id;
            boardIdx.Value = go.boardIndex;
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
            var treasuresReference = SystemAPI.GetSingleton<TreasuresDataBlobReference>().treasuresBlobReference;
            var goToJailReference = SystemAPI.GetSingleton<GoToJailDataBlobReference>().goToJailBlobReference;
            var chancesReference = SystemAPI.GetSingleton<ChancesDataBlobReference>().chancesBlobReference;
            var parkingReference = SystemAPI.GetSingleton<ParkingDataBlobReference>().parkingBlobReference;
            var taxesReference = SystemAPI.GetSingleton<TaxesDataBlobReference>().taxesBlobReference;
            var jailReference = SystemAPI.GetSingleton<JailDataBlobReference>().jailBlobReference;
            var goReference = SystemAPI.GetSingleton<GoDataBlobReference>().goBlobReference;

            new InitializePropertiesJob { propertiesReference = propertiesReference }.ScheduleParallel();
            new InitializeTreasuresJob { treasuresReference = treasuresReference }.ScheduleParallel();
            new InitializeGoToJailJob { goToJailReference = goToJailReference }.ScheduleParallel();
            new InitializeChancesJob { chancesReference = chancesReference }.ScheduleParallel();
            new InitializeParkingJob { parkingReference = parkingReference }.ScheduleParallel();
            new InitializeTaxesJob { taxesReference = taxesReference }.ScheduleParallel();
            new InitializeJailJob { jailReference = jailReference }.ScheduleParallel();
            new InitializeGoJob { goReference = goReference }.ScheduleParallel();
        }
    }
}
