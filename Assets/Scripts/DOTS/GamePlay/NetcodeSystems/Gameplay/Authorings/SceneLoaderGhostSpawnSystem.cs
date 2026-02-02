using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SceneLoaderGhostSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneLoaderGhostReference>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var sceneLoaderPrefab = SystemAPI.GetSingleton<SceneLoaderGhostReference>().Entity;

            state.EntityManager.Instantiate(sceneLoaderPrefab);

            state.Enabled = false;
        }
    }
}
