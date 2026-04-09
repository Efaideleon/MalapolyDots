using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateServerScenesSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SceneLoader>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.EntityManager.CreateSingleton(new ServerSceneLoadState { GameSceneRequested = false });
        }

        public void OnUpdate(ref SystemState state)
        {
            var scenes = SystemAPI.GetSingleton<SceneLoader>();
            var gamePhase = SystemAPI.GetSingleton<GamePhaseGhostComponent>();
            ref var loadState = ref SystemAPI.GetSingletonRW<ServerSceneLoadState>().ValueRW;

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
                        SceneSystem.LoadSceneAsync(state.WorldUnmanaged, scenes.SceneEntityReference);
                        loadState.GameSceneRequested = true;
                    }
                    break;
            }
        }
    }

    public struct ServerSceneLoadState : IComponentData
    {
        public bool GameSceneRequested;
    }
}

