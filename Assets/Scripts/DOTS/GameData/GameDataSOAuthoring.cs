using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class GameDataSOAuthoring : MonoBehaviour
    {
        [SerializeField] GameData gameData;

        public class GameDataBlobBaker : Baker<GameDataSOAuthoring>
        {
            public override void Bake(GameDataSOAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                NativeArray<FixedString32Bytes> charactersSelectedNativeArr = new (
                    authoring.gameData.charactersSelected.Count,
                    Allocator.Persistent
                );

                for (int i = 0; i < authoring.gameData.charactersSelected.Count; i++)
                {
                    FixedString32Bytes fixedString = default;
                    fixedString.CopyFrom(authoring.gameData.charactersSelected[i]);
                    charactersSelectedNativeArr[i] = fixedString;
                }

                AddComponent(entity, new GameDataComponent
                {
                    NumberOfRounds = authoring.gameData.numberOfRounds,
                    NumberOfPlayers = authoring.gameData.numberOfPlayers,
                    CharactersSelected = charactersSelectedNativeArr
                });
            }
        }
    }
}
