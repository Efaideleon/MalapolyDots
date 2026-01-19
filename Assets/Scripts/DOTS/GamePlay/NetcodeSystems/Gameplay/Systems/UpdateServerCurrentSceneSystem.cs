using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateServerCurrentSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new CurrentScene { sceneGUID = default });
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var sceneReference in SystemAPI.Query<RefRO<SceneReference>>())
            {
                var currentScene = SystemAPI.GetSingleton<CurrentScene>();
                if (currentScene.sceneGUID != sceneReference.ValueRO.SceneGUID)
                {
                    SystemAPI.GetSingletonRW<CurrentScene>().ValueRW.sceneGUID = sceneReference.ValueRO.SceneGUID;
                }
            }
        }
    }

    public struct CurrentScene : IComponentData
    {
        public Hash128 sceneGUID;
    }
}
