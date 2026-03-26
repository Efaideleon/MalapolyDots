using DOTS.GameSpaces;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct EnsureAllPlacesSpawnedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PlacesPrefabBuffer>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingletonRW<GameStateComponent>();
            var placesPrefabs = SystemAPI.GetSingletonBuffer<PlacesPrefabBuffer>();

            var numberOfPlaces = placesPrefabs.Length;
            int count = 0;

            foreach (var _ in SystemAPI.Query<RefRO<PlaceInstantiatedTag>>())
            {
                count++;
                if (count == numberOfPlaces)
                {
                    gameState.ValueRW.AllPlacesInstantiated = true;
                    UnityEngine.Debug.Log($"[PlacesSpawner] | count ==  numberOfPlaces {count}, {numberOfPlaces} all places spawned");
                    state.Enabled = false;
                }
            }
        }
    }

    public struct AllPlacesInstantiatedTag : IComponentData
    { }
}
