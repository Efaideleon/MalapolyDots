using Assets.Scripts.DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClientChangeSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalScene>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            bool sceneChangeRequest = false;
            Hash128 targetScene = default;

            foreach (var (rpc, newScene, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<SendSceneChangeDataRpc>>().WithEntityAccess())
            {
                var sharedSceneGUID = newScene.ValueRO.SceneGUID;

                sceneChangeRequest = true;
                targetScene = sharedSceneGUID;

                ecb.DestroyEntity(rpcEntity);
            }

            if (sceneChangeRequest)
            {
                var localScene = SystemAPI.GetSingleton<LocalScene>().sceneGUID;

                if (localScene != targetScene)
                {
                    UnityEngine.Debug.Log($"[ClientChangeSceneSystem] | There is a change in scene in server, updating in client...");
                    UnityEngine.Debug.Log($"[ClientChangeSceneSystem] | local Scene guid: {localScene}, new scene guid: {targetScene}");
                    //SceneSystem.UnloadScene(state.WorldUnmanaged, localScene);
                    SceneSystem.LoadSceneAsync(state.WorldUnmanaged, targetScene);

                    // Check if what scene we are in first.
                    var gamemenuui = SystemAPI.ManagedAPI.GetSingleton<GameObjectReference>().Instance;
                    UnityEngine.GameObject.Destroy(gamemenuui);

                    SystemAPI.GetSingletonRW<LocalScene>().ValueRW.sceneGUID = targetScene;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
