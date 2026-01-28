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
                        foreach (var (_, e) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                        {
                            UnityEngine.Debug.Log(
                                    $"[SERVER] Before scene load | Conn {e} InGame=" +
                                    state.EntityManager.HasComponent<NetworkStreamInGame>(e)
                                    );
                        }
                        SceneSystem.LoadSceneAsync(state.WorldUnmanaged, scenes.GameSceneGUID);
                        state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<GameMenuTag>());
                        UnityEngine.Debug.Log($"[UpdateClientScenesSystem] | Setting Client Game Scene, Deleting StartMenuUIGameObject");
                        loadState.GameSceneRequested = true;
                    }
                    break;
            }
        }
    }

    public struct ClientSceneLoadState : IComponentData
    {
        public bool GameSceneRequested;
    }
}

