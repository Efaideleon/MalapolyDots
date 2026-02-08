using Unity.Entities;
using Unity.Scenes;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SetupStartSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentScene>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // if (SystemAPI.HasSingleton<SetupLoadSceneTag>())
            //     return;
            //
            // var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            // foreach (var (sceneReference, entity) in SystemAPI.Query<RefRO<SceneReference>>().WithEntityAccess())
            // {
            //     UnityEngine.Debug.Log($"[SetupStartSceneSystem] | Running.");
            //     if (SceneSystem.IsSceneLoaded(state.WorldUnmanaged, entity))
            //     {
            //         UnityEngine.Debug.Log($"[SetupStartSceneSystem] | Getting current scene.");
            //
            //         var tagEntity = ecb.CreateEntity();
            //         ecb.AddComponent<SetupLoadSceneTag>(tagEntity);
            //         break;
            //     }
            // }
            //
            // ecb.Playback(state.EntityManager);
            // ecb.Dispose();
        }
    }

    public struct SetupLoadSceneTag : IComponentData
    { }
}
