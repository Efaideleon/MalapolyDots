using Unity.Entities;
namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateLocalCurrentSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneReference>();
            state.EntityManager.CreateSingleton(new LocalScene { sceneGUID = default });
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var sceneReference in SystemAPI.Query<RefRO<SceneReference>>())
            {
                var localScene = SystemAPI.GetSingleton<LocalScene>();
                if (localScene.sceneGUID != sceneReference.ValueRO.SceneGUID)
                {
                    SystemAPI.GetSingletonRW<LocalScene>().ValueRW.sceneGUID = sceneReference.ValueRO.SceneGUID;
                }
            }
        }

    }

    public struct LocalScene : IComponentData
    {
        public Hash128 sceneGUID;
    }
}
