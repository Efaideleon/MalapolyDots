using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using TitleScreen.NetworkUI.Systems;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct UpdateClientScenesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneLoader>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.EntityManager.CreateSingleton(new ClientSceneLoadState { GameSceneRequested = false });
            UnityEngine.Debug.Log("[UpdateClientScenesSystem] | OnCreate");
        }

        public void OnUpdate(ref SystemState state)
        {
            var scenes = SystemAPI.GetSingleton<SceneLoader>();
            var gamePhase = SystemAPI.GetSingleton<GamePhaseGhostComponent>();
            ref var loadState = ref SystemAPI.GetSingletonRW<ClientSceneLoadState>().ValueRW;

            // Guard against default/invalid GUIDs that can occur before ghost data is fully replicated.
            if (scenes.GameSceneGuid == default)
            {
                return;
            }

            switch (gamePhase.GamePhase)
            {
                case GamePhase.Game:
                    if (!loadState.GameSceneRequested)
                    {
                        var sceneEntity = SceneSystem.LoadSceneAsync(state.WorldUnmanaged, scenes.GameSceneGuid);
                        // Only mark as requested if the load call returned a valid scene entity.
                        if (sceneEntity != Entity.Null)
                        {
                            loadState.GameSceneRequested = true;
                            loadState.SceneEntity = sceneEntity;
                            UnityEngine.Debug.Log($"[UpdateClientScenesSystem] | Scene loaded correctly");
                        }
                        else if (!loadState.LoggedNullSceneEntity)
                        {
                            UnityEngine.Debug.Log("[UpdateClientScenesSystem] | LoadSceneAsync returned Entity.Null.");
                            loadState.LoggedNullSceneEntity = true;
                        }
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
        public bool LoggedNullSceneEntity;
    }

    public struct ClientSceneDebugState : IComponentData
    {
        public bool Logged;
    }
}
