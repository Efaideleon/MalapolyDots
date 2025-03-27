using Unity.Collections;
using Unity.Entities;

public static class GoToJailDataBlobBuilder
{
    public static BlobAssetReference<GoToJailDataBlob> Create(GoToJailData goToJailData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref GoToJailDataBlob root = ref builder.ConstructRoot<GoToJailDataBlob>();

        root.goToJail.id= goToJailData.id;
        root.goToJail.Name = goToJailData.Name;
        root.goToJail.boardIndex = goToJailData.boardIndex;

        BlobAssetReference<GoToJailDataBlob> blobReference = builder.CreateBlobAssetReference<GoToJailDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}
