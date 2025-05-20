using System.Collections.Generic;
using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Utilities.PropertiesBlob
{
    public struct PropertiesDataBlobBuilder
    {
        public static readonly Dictionary<string, PropertyColor> ColorMap = new()
        {
            { "None", PropertyColor.White },
            { "Brown", PropertyColor.Brown },
            { "LightBlue", PropertyColor.LightBlue },
            { "Purple", PropertyColor.Purple },
            { "Orange", PropertyColor.Orange },
            { "Red", PropertyColor.Red },
            { "Yellow", PropertyColor.Yellow },
            { "Green", PropertyColor.Green },
            { "Blue", PropertyColor.Blue }
        };

        public static BlobAssetReference<PropertiesDataBlob> Create(PropertyData[] propertiesData, IBaker baker)
        {
            return GenericBlobAssetBuilder.CreateBlobAsset(baker,
                    (BlobBuilder builder, ref PropertiesDataBlob root) =>
                    {
                        // Using the builder to allocate memory to the BlobArray in the Blob Asset reference.
                        BlobBuilderArray<FixedPropertyData> propertiesBuilder = builder.Allocate(ref root.properties, propertiesData.Length);

                        for (int i = 0; i < propertiesData.Length; i++)
                        {
                            PropertyData property = propertiesData[i];
                            var propertyRentArray = propertiesData[i].rent;

                            // Allocate memroy for the rent array in the propertiesBuilder array.
                            BlobBuilderArray<int> rentBuilder = builder.Allocate(ref propertiesBuilder[i].rent, propertyRentArray.Length);
                            for (int j = 0; j < propertyRentArray.Length; j++)
                            {
                                rentBuilder[j] = propertyRentArray[j];
                            }
                            propertiesBuilder[i].id = property.id;
                            propertiesBuilder[i].name = property.Name;
                            propertiesBuilder[i].boardIndex = property.boardIndex;
                            propertiesBuilder[i].price = property.price;
                            propertiesBuilder[i].rentWithHotel = property.rentWithHotel;
                            propertiesBuilder[i].color = ColorMap[property.color];
                        }
                    });
        }
    }
}
