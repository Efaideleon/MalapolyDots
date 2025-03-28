using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class GameDataSOAuthoring : MonoBehaviour
    {
        [SerializeField] GameData gameDataSO;

        void Start()
        {
            Debug.Log("Start in GameDataSOAuthoring");
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
