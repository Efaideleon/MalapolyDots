using Unity.Entities;

namespace DOTS.Utilities.GoBlob
{
    public struct GoDataBlobBuilder
    {
        public static BlobAssetReference<GoDataBlob> Create(GoData goData, IBaker baker)
        {
            return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
                    (BlobBuilder builder, ref GoDataBlob root) => 
                    {
                    root.go.id = goData.id;
                    root.go.Name = goData.Name;
                    root.go.boardIndex = goData.boardIndex;
                    });
        }
    }
}
