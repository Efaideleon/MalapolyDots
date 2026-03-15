using Unity.Entities;

namespace Assets.Scripts.DOTS.DataComponents
{
    public struct CurrentScene : IComponentData
    {
        public Hash128 sceneGUID;
    }
}
