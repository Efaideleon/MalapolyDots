using Unity.Entities;

namespace Assets.Scripts.DOTS.DataComponents
{
    public struct SceneLoader : IComponentData
    {
        public Hash128 GameSceneGuid;
    }
}
