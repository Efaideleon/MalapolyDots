using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using TitleScreen.NetworkUI.Systems;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateClientScenesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneLoader>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.EntityManager.CreateSingleton(new ClientSceneLoadState { GameSceneRequested = false });
        }

        public void OnUpdate(ref SystemState state)
        {
            var scenes = SystemAPI.GetSingleton<SceneLoader>();
            var gamePhase = SystemAPI.GetSingleton<GamePhaseGhostComponent>();
            ref var loadState = ref SystemAPI.GetSingletonRW<ClientSceneLoadState>().ValueRW;

            switch (gamePhase.GamePhase)
            {
                case GamePhase.Game:
                    if (!loadState.GameSceneRequested)
                    {
                        var sceneEntity = SceneSystem.LoadSceneAsync(state.WorldUnmanaged, scenes.GameSceneGuid);
                        loadState.GameSceneRequested = true;
                        loadState.SceneEntity = sceneEntity;
                    }
                    break;
            }

            if (!loadState.GameSceneLoaded && loadState.SceneEntity != Entity.Null && SceneSystem.IsSceneLoaded(state.WorldUnmanaged, loadState.SceneEntity))
            {
                UnityEngine.Debug.Log("[Client] Game scene fully loaded");
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<GameMenuTag>());
                UnityEngine.Debug.Log($"[UpdateClientScenesSystem] | Setting Client Game Scene, Deleting StartMenuUIGameObject");
                loadState.GameSceneLoaded = true;
            }
        }
    }

    public struct ClientGameSceneLoadedRPC : IRpcCommand
    { }

    public struct ClientSceneLoadState : IComponentData
    {
        public bool GameSceneRequested;
        public bool GameSceneLoaded;
        public Entity SceneEntity;
    }
}

