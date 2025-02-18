using Unity.Entities;

namespace DOTS
{
    public struct GameDataBlobComponent : IComponentData
    {
        public BlobAssetReference<GameDataBlob> gameDataBlob;
    }

    public class GameDataBlobBaker : Baker<GameDataSOAuthoring>
    {
        public override void Bake(GameDataSOAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GameDataBlobComponent { gameDataBlob = authoring.GameDataBlobAssetReference });
        }
    }
}
