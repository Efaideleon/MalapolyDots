using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CharactersAuthoring : MonoBehaviour
{
    [Header("Characters Names")] 
    [SerializeField] public string[] characterNames;
    
    [Header("Character Prefabs")]
    [SerializeField] public GameObject[] characterPrefabs;

    class CharactersBaker : Baker<CharactersAuthoring>
    {
        public override void Bake(CharactersAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            for (int i = 0; i < authoring.characterNames.Length; i++)
            {
                var characterPrefab = authoring.characterPrefabs[i];
                var characterName = authoring.characterNames[i];
                var charEntity = GetEntity(characterPrefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameDataComponent { Name = characterName });
                AddComponent(entity, new PrefabTag());
            }
        }
    }
}

public struct CharactersEntities : IComponentData
{
    public Entity AvocadoEntity;
    public Entity LiraEntity;
    public Entity CoinEntity;
    public Entity MugEntity;
    public Entity BirdEntity;
    public Entity TucTucEntity;
}