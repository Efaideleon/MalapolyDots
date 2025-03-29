using Unity.Entities;

public static class JailDataBlobBuilder
{
    public static BlobAssetReference<JailDataBlob> Create(JailData jailData, IBaker baker)
    {
        return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
        (BlobBuilder builder, ref JailDataBlob root) => 
        {
            root.jail.id= jailData.id;
            root.jail.Name = jailData.Name;
            root.jail.boardIndex = jailData.boardIndex;
        });
    }
}
