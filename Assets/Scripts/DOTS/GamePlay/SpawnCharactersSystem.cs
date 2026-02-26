using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Unity.Jobs;
using Unity.NetCode;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using Assets.Scripts.DOTS.Characters;

namespace DOTS.Characters.CharacterSpawner
{
    //[BurstCompile]
    public partial struct SpawnParallelJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        [ReadOnly]
        public NativeArray<float3> Positions;

        [ReadOnly]
        public NativeList<CharacterToSpawn> Prefabs;

        [BurstCompile]
        public void Execute(int index)
        {
            InstantiatePrefab(ecbParallel, Prefabs[index], Positions[index], index);
        }

        [BurstCompile]
        private void InstantiatePrefab(EntityCommandBuffer.ParallelWriter ecbParallel, CharacterToSpawn characterToSpawn, float3 position, int sort_key)
        {
            var instance = ecbParallel.Instantiate(sort_key, characterToSpawn.Prefab);
            ecbParallel.SetComponent(sort_key, instance, new LocalTransform
            {
                Rotation = quaternion.identity,
                Position = position,
                Scale = 1f
            });

            ecbParallel.SetComponent(sort_key, instance, new GhostOwner { NetworkId = characterToSpawn.OwnerNetworkId });
            ecbParallel.SetComponent(sort_key, instance, new PlayerID { Value = sort_key });
        }
    }

    //[BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SpawnCharactersSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.RequireForUpdate<SpawnPointComponent>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<GeneralGhostStates>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<CharactersSpawnedTag>())
                return;

            if (SystemAPI.GetSingleton<GamePhaseGhostComponent>().GamePhase == GamePhase.Game)
            {
                NativeList<CharacterToSpawn> charactersPicked = new(Allocator.TempJob);
                foreach (var character in SystemAPI.Query<RefRO<PrepickedCharacter>>())
                {
                    if (character.ValueRO.PrePicked != default)
                    {
                        charactersPicked.Add(new CharacterToSpawn
                        {
                            Prefab = character.ValueRO.Prefab,
                            OwnerNetworkId = character.ValueRO.OwnerNetworkId
                        });
                    }
                }

                var spawnCenter = SystemAPI.GetSingleton<SpawnPointComponent>().Position;
                var numOfCharactersPicked = charactersPicked.Length;
                var positions = new NativeArray<float3>(numOfCharactersPicked, Allocator.TempJob);
                var spawnRadius = 4;
                CalculatePositions(positions, spawnCenter, spawnRadius);

                var job = new SpawnParallelJob
                {
                    ecbParallel = GetECB(ref state).AsParallelWriter(),
                    Prefabs = charactersPicked,
                    Positions = positions
                };

                var jobHandle = job.Schedule(numOfCharactersPicked, 5);
                jobHandle = positions.Dispose(jobHandle);
                jobHandle = charactersPicked.Dispose(jobHandle);

                state.Dependency = jobHandle;
                state.EntityManager.CreateSingleton<CharactersSpawnedTag>();
            }
        }

        [BurstCompile]
        private readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }

        private readonly void CalculatePositions(NativeArray<float3> positions, float3 spawnPosition, float radius)
        {
            float angleStep = math.PI * 2f / positions.Length;

            for (int i = 0; i < positions.Length; i++)
            {
                float angle = i * angleStep;
                // following cartesian plane
                float x = math.cos(angle) * radius;
                float y = math.sin(angle) * radius;

                // following unity plane
                positions[i] = new float3(x, 0f, y) + spawnPosition;
            }
        }
    }

    public struct CharacterToSpawn
    {
        public Entity Prefab;
        public int OwnerNetworkId;
    }

    public struct CharactersSpawnedTag : IComponentData
    { }
}
