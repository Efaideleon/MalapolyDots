using Unity.Entities;
using UnityEngine;

namespace DOTS.GameData
{
    public class GameDataSOAuthoring : MonoBehaviour
    {
        [SerializeField] Gamedata gameDataSO;

        void Start()
        {
            Debug.Log("[GameDataSOAuthoring] | Start in GameDataSOAuthoring");
            EntityManager entityManger = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManger.CreateEntity();

            var gameDataComponent = new GameDataComponent
            {
                NumberOfRounds = gameDataSO.numberOfRounds,
                NumberOfPlayers = gameDataSO.numberOfPlayers,
            };

            var buffer = entityManger.AddBuffer<CharacterSelectedBuffer>(entity);
            foreach (var characterSelected in gameDataSO.charactersSelected)
            {
                buffer.Add(new CharacterSelectedBuffer
                {
                    Value = characterSelected,
                });
            }

            entityManger.AddComponent<GameDataComponent>(entity);
            entityManger.SetComponentData(entity, gameDataComponent);
        }
    }
}
