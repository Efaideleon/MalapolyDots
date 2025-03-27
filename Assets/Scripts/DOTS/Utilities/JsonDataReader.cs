using Unity.Entities;
using UnityEngine;

[System.Serializable]
public class PropertyData
{
    public int id;
    public string Name;
    public int boardIndex;
    public int price;
    public int[] rent;
    public int rentWithHotel;
}

[System.Serializable]
public class ChanceData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class GoData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class GoToJailData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class JailData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class ParkingData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class TaxData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class TreasureData
{
    public int id;
    public string Name;
    public int boardIndex;
}

[System.Serializable]
public class SpacesData
{
    public PropertyData[] properties;
    public ChanceData[] chances;
    public GoData go;
    public GoToJailData goToJail;
    public JailData jail;
    public ParkingData parking;
    public TaxData[] taxes;
    public TreasureData[] treasures;
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
                Debug.Log("JSON string is empty, check the file loading");
            }
            else
            {
                SpacesData spacesData = JsonUtility.FromJson<SpacesData>(spacesDataString);
                // Create a BlobAsset
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var propertiesBlobReference = PropertiesDataBlobBuilder.Create(spacesData.properties);
                var treasuresBlobReference = TreasureDataBlobBuilder.Create(spacesData.treasures);
                var goToJailBlobReference = GoToJailDataBlobBuilder.Create(spacesData.goToJail);
                var chancesBlobReference = ChancesDataBlobBuilder.Create(spacesData.chances);
                var parkingBlobReference = ParkingDataBlobBuilder.Create(spacesData.parking);
                var taxesBlobReference = TaxesDataBlobBuilder.Create(spacesData.taxes);
                var jailBlobReference = JailDataBlobBuilder.Create(spacesData.jail);
                var goBlobReference = GoDataBlobBuilder.Create(spacesData.go);

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
