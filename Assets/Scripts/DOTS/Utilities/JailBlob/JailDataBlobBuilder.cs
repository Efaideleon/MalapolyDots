using Unity.Collections;
using Unity.Entities;

public static class JailDataBlobBuilder
{
    public static BlobAssetReference<JailDataBlob> Create(JailData jailData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref JailDataBlob root = ref builder.ConstructRoot<JailDataBlob>();

        root.jail.id= jailData.id;
        root.jail.Name = jailData.Name;
        root.jail.boardIndex = jailData.boardIndex;

        BlobAssetReference<JailDataBlob> blobReference = builder.CreateBlobAssetReference<JailDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}
