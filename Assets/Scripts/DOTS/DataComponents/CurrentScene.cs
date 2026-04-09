using Unity.Entities;
using Unity.Entities.Serialization;

namespace Assets.Scripts.DOTS.DataComponents
{
    public struct CurrentScene : IComponentData
    {
        public SceneReference sceneEntityReference;
    }
}
