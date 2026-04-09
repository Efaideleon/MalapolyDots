using Unity.Entities;
using Unity.Entities.Serialization;

namespace Assets.Scripts.DOTS.DataComponents
{
    public struct SceneLoader : IComponentData
    {
        public EntitySceneReference SceneEntityReference;
    }
}
