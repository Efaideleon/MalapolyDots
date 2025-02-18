using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class GameDataSOAuthoring : MonoBehaviour
    {
        [SerializeField] GameData gameData;
        public BlobAssetReference<GameDataBlob> GameDataBlobAssetReference { get; private set; }

        void Awake()
        {
            GameDataBlobAssetReference = GameDataBlobBuilder.Create(gameData);
        }

        void OnDestroy()
        {
            if (GameDataBlobAssetReference.IsCreated)
            {
                GameDataBlobAssetReference.Dispose();
            }
        }
    }
}
