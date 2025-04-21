using Unity.Collections;
using Unity.Entities;

namespace DOTS.GameData
{
    public static class GameDataBlobBuilder
    {
        public static BlobAssetReference<GameDataBlob> Create(Gamedata gameData)
        {
            // Builder to help us allocate memory for elements in the blob asset
            // and also allocate memory for the blob asset itself.
            var builder = new BlobBuilder(Allocator.Temp);

            // Helps us get a reference to where we are going to allocate memory to.
            ref GameDataBlob root = ref builder.ConstructRoot<GameDataBlob>();

            root.NumberOfPlayers = gameData.numberOfPlayers;
            root.NumberOfRounds = gameData.numberOfRounds;

            // Using the builder to allocate memory to the BlobArray in the Blob Asset reference.
            BlobBuilderArray<BlobString> blobArray = builder.Allocate(ref root.CharactersSelected, gameData.charactersSelected.Count);

            for (int i = 0; i < gameData.charactersSelected.Count; i++)
            {
                string currentString = gameData.charactersSelected[i];
                // Using the builder to allocate memory to the BlobStrings in the BlobArray inside the blob asset.
                builder.AllocateString(ref blobArray[i], currentString);
            }

            // Actually allocating the memory for the BlobAsset and making it persistent.
            BlobAssetReference<GameDataBlob> blobReference = builder.CreateBlobAssetReference<GameDataBlob>(Allocator.Persistent);
            builder.Dispose();
            return blobReference;
        }
    }
}
