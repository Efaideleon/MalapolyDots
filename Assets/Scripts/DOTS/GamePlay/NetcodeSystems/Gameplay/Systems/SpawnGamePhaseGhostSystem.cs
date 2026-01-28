using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SpawnGamePhaseGhostSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GamePhaseGhostReference>();
        }
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GamePhaseGhostSpawnedFlag>())
                return;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var gamePhaseGhostRef in SystemAPI.Query<RefRO<GamePhaseGhostReference>>())
            {
                var ghostPrefab = ecb.Instantiate(gamePhaseGhostRef.ValueRO.PrefabToSpawn);
                ecb.SetComponent(ghostPrefab, new GamePhaseGhostComponent { GamePhase = GamePhase.StartMenu });

                var entity = ecb.CreateEntity();
                ecb.AddComponent<GamePhaseGhostSpawnedFlag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct GamePhaseGhostSpawnedFlag : IComponentData
    { }
}
