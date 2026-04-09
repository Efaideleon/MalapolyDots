using Assets.Scripts.DOTS.DataComponents;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateServerCurrentSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new CurrentScene { sceneEntityReference = default });
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var sceneReference in SystemAPI.Query<RefRO<SceneReference>>())
            {
                var currentScene = SystemAPI.GetSingleton<CurrentScene>();
                if (!currentScene.sceneEntityReference.Equals(sceneReference.ValueRO))
                {
                    SystemAPI.GetSingletonRW<CurrentScene>().ValueRW.sceneEntityReference = sceneReference.ValueRO;
                }
            }
        }
    }

}
