using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MoveToNextSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RequestSceneChange>();
            state.RequireForUpdate<SceneLoader>();
            state.RequireForUpdate<CurrentSharedScene>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var sceneGUID = SystemAPI.GetSingleton<SceneLoader>().SceneGUID;
            var currentScene = SystemAPI.GetSingleton<CurrentScene>();

            if (currentScene.sceneGUID != default)
            {
                UnityEngine.Debug.Log($"[MoveToNextSceneSystem] | unloading the current scene: {currentScene.sceneGUID}");
                //SceneSystem.UnloadScene(state.WorldUnmanaged, currentScene.sceneGUID);
            }

            var rpcEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<SendRpcCommandRequest>(rpcEntity);
            state.EntityManager.AddComponent<SendSceneChangeDataRpc>(rpcEntity);
            state.EntityManager.AddComponentData(rpcEntity, new SendSceneChangeDataRpc { SceneGUID = sceneGUID });

            var entity = SystemAPI.GetSingletonEntity<RequestSceneChange>();
            state.EntityManager.DestroyEntity(entity);

            UnityEngine.Debug.Log($"[MoveToNextSceneSystem] | loading the next scene");

            SceneSystem.LoadSceneAsync(state.WorldUnmanaged, sceneGUID);
        }
    }

    public struct RequestSceneChange : IComponentData
    { }

    public struct SendSceneChangeDataRpc : IRpcCommand
    {
        public Hash128 SceneGUID;
    }
}

