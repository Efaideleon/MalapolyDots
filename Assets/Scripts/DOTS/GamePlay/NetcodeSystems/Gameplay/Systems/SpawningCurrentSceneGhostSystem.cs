using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SpawningCurrentSceneGhostSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentSceneGhostReference>();
            state.EntityManager.CreateSingleton(new CurrentSharedScene { SceneGUID = default });
        }

        public void OnUpdate(ref SystemState state)
        {
        //     if (SystemAPI.HasSingleton<CurrentSceneGhostSpawnedTag>())
        //         return;
        //
        //     var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        //     foreach (var ghostRef in SystemAPI.Query<RefRO<CurrentSceneGhostReference>>())
        //     {
        //         ecb.Instantiate(ghostRef.ValueRO.entity);
        //
        //         var tagEntity = ecb.CreateEntity();
        //
        //         ecb.AddComponent<CurrentSceneGhostSpawnedTag>(tagEntity);
        //     }
        //
        //     ecb.Playback(state.EntityManager);
        //     ecb.Dispose();
        }
    }

    public struct CurrentSceneGhostSpawnedTag : IComponentData
    { }

}
