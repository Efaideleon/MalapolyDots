using Unity.Entities;

public static class GoToJailDataBlobBuilder
{
    public static BlobAssetReference<GoToJailDataBlob> Create(GoToJailData goToJailData, IBaker baker)
    {
        return GenericBlobAssetBuilder.CreateBlobAsset(baker,
        (BlobBuilder builder, ref GoToJailDataBlob root) =>
        {
            root.goToJail.id= goToJailData.id;
            root.goToJail.Name = goToJailData.Name;
            root.goToJail.boardIndex = goToJailData.boardIndex;
        });
    }
}
