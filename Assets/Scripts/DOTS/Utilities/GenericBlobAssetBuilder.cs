using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Utilities
{
    public delegate void BlobBuildAction<TDataBlob>(BlobBuilder blobBuilder, ref TDataBlob data) where TDataBlob : unmanaged;

    public struct GenericBlobAssetBuilder
    {
        public static BlobAssetReference<TDataBlob> CreateBlobAsset<TDataBlob>(
                IBaker baker,
                BlobBuildAction<TDataBlob> blobAction
                ) where TDataBlob : unmanaged
        {
            // Builder to help us allocate memory for elements in the blob asset
            // and also allocate memory for the blob asset itself.
            var builder = new BlobBuilder(Allocator.Temp);

            // Helps us get a reference to where we are going to allocate memory to.
            ref TDataBlob root = ref builder.ConstructRoot<TDataBlob>();

            blobAction(builder, ref root);

            // Actually allocating the memory for the BlobAsset and making it persistent.
            BlobAssetReference<TDataBlob> blobReference = builder.CreateBlobAssetReference<TDataBlob>(Allocator.Persistent);
            builder.Dispose();
            baker.AddBlobAsset(ref blobReference, out var _);
            return blobReference;
        }
    }
}
