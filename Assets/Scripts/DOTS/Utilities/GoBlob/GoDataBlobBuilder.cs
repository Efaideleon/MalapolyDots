using Unity.Collections;
using Unity.Entities;

public static class GoDataBlobBuilder
{
    public static BlobAssetReference<GoDataBlob> Create(GoData goData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref GoDataBlob root = ref builder.ConstructRoot<GoDataBlob>();

        root.go.id= goData.id;
        root.go.Name = goData.Name;
        root.go.boardIndex = goData.boardIndex;

        BlobAssetReference<GoDataBlob> blobReference = builder.CreateBlobAssetReference<GoDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}
