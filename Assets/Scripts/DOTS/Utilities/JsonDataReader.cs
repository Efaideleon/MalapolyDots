using DOTS.Utilities.ChancesBlob;
using DOTS.Utilities.GoBlob;
using DOTS.Utilities.GoToJailBlob;
using DOTS.Utilities.JailBlob;
using DOTS.Utilities.ParkingBlob;
using DOTS.Utilities.PropertiesBlob;
using DOTS.Utilities.TaxesBlob;
using DOTS.Utilities.TreasuresBlob;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Utilities
{
    [System.Serializable]
    public struct PropertyData
    {
        public int id;
        public string Name;
        public int boardIndex;
        public int price;
        public int[] rent;
        public int rentWithHotel;
        public string color;
    }

    [System.Serializable]
    public struct ChanceData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct GoData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct GoToJailData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct JailData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct ParkingData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct TaxData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct TreasureData
    {
        public int id;
        public string Name;
        public int boardIndex;
    }

    [System.Serializable]
    public struct TreasureCardData
    {
        public int id;
        public string data;
    }

    [System.Serializable]
    public struct TreasuresData
    {
        public TreasureData[] treasures;
        public TreasureCardData[] cards;
    }

    [System.Serializable]
    public struct SpacesData
    {
        public PropertyData[] properties;
        public ChanceData[] chances;
        public GoData go;
        public GoToJailData goToJail;
        public JailData jail;
        public ParkingData parking;
        public TaxData[] taxes;
        public TreasuresData treasures;
    }

    public class JsonDataReader : MonoBehaviour
    {
        [SerializeField] TextAsset spacesDataJsonFile;

        public class JsonDataBaker : Baker<JsonDataReader>
        {
            public override void Bake(JsonDataReader authoring)
            {
                string spacesDataString = "";
                if (authoring.spacesDataJsonFile != null)
                {
                    spacesDataString = authoring.spacesDataJsonFile.text;
                }

                if (string.IsNullOrEmpty(spacesDataString))
                {
                    Debug.Log("[JsonDataReader] | JSON string is empty, check the file loading");
                }
                else
                {
                    SpacesData spacesData = JsonUtility.FromJson<SpacesData>(spacesDataString);
                    // Create a BlobAsset
                    var entity = GetEntity(authoring, TransformUsageFlags.None);

                    var propertiesBlobReference = PropertiesDataBlobBuilder.Create(spacesData.properties, this);
                    var treasuresBlobReference = TreasureDataBlobBuilder.Create(spacesData.treasures, this);
                    var goToJailBlobReference = GoToJailDataBlobBuilder.Create(spacesData.goToJail, this);
                    var chancesBlobReference = ChancesDataBlobBuilder.Create(spacesData.chances, this);
                    var parkingBlobReference = ParkingDataBlobBuilder.Create(spacesData.parking, this);
                    var taxesBlobReference = TaxesDataBlobBuilder.Create(spacesData.taxes, this);
                    var jailBlobReference = JailDataBlobBuilder.Create(spacesData.jail, this);
                    var goBlobReference = GoDataBlobBuilder.Create(spacesData.go, this);

                    var propertiesDataBlobComponent = new PropertiesDataBlobReference
                    {
                        propertiesBlobReference = propertiesBlobReference
                    };
                    var treasuresDataBlobComponent = new TreasuresDataBlobReference
                    {
                        treasuresBlobReference = treasuresBlobReference
                    };
                    var goToJailDataBlobComponent = new GoToJailDataBlobReference
                    {
                        goToJailBlobReference = goToJailBlobReference
                    };
                    var chancesDataBlobComponent = new ChancesDataBlobReference
                    {
                        chancesBlobReference = chancesBlobReference
                    };
                    var parkingDataBlobComponent = new ParkingDataBlobReference
                    {
                        parkingBlobReference = parkingBlobReference
                    };
                    var taxesDataBlobComponent = new TaxesDataBlobReference
                    {
                        taxesBlobReference = taxesBlobReference
                    };
                    var jailDataBlobComponent = new JailDataBlobReference
                    {
                        jailBlobReference = jailBlobReference
                    };
                    var goDataBlobComponent = new GoDataBlobReference
                    {
                        goBlobReference = goBlobReference
                    };

                    AddComponent(entity, propertiesDataBlobComponent);
                    AddComponent(entity, treasuresDataBlobComponent);
                    AddComponent(entity, goToJailDataBlobComponent);
                    AddComponent(entity, chancesDataBlobComponent);
                    AddComponent(entity, parkingDataBlobComponent);
                    AddComponent(entity, taxesDataBlobComponent);
                    AddComponent(entity, jailDataBlobComponent);
                    AddComponent(entity, goDataBlobComponent);
                }
            }
        }
    }
}
