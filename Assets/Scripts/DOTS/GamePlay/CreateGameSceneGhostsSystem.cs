using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CreateGameSceneGhostsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateGhostReference>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GameStateComponent>())
                return;

            // if (SystemAPI.HasSingleton<CurrentPlayerID>())
                //return;

            if (SystemAPI.HasSingleton<CurrentActivePlayer>())
                return;

            UnityEngine.Debug.Log($"[InstantiateGameStateGhostSystem] | creating gamestate ghost prefab.");

            var ghostPrefab = SystemAPI.GetSingleton<GameStateGhostReference>().Entity;
            state.EntityManager.Instantiate(ghostPrefab);

            // var currentPlayerIDGhostPrefab = SystemAPI.GetSingleton<CurrentPlayerIDGhostReference>().Entity;
            // state.EntityManager.Instantiate(currentPlayerIDGhostPrefab);

            var currentActivePlayerGhostPrefab = SystemAPI.GetSingleton<CurrentActivePlayerGhostReference>().Entity;
            state.EntityManager.Instantiate(currentActivePlayerGhostPrefab);
        }
    }
}
