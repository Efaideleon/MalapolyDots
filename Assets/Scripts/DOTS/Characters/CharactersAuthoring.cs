using Unity.Entities;
using UnityEngine;

public class CharactersAuthoring : MonoBehaviour
{
    [SerializeField] public string charName;
    [SerializeField] public GameObject prefab;

    class CharactersBaker : Baker<CharactersAuthoring>
    {
        public override void Bake(CharactersAuthoring authoring)
        {
            var authoringEntity = GetEntity(authoring, TransformUsageFlags.None);
            var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);

            AddComponent(authoringEntity, new PrefabReferenceComponent { Value = prefabEntity });
            AddComponent(authoringEntity, new NameDataComponent{ Name = authoring.charName });
            AddComponent(authoringEntity, new PrefabTag());
        }
    }
}

public struct PrefabReferenceComponent : IComponentData
{
    public Entity Value;
}
