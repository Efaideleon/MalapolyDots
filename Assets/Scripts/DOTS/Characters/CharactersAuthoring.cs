using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class CharactersAuthoring : MonoBehaviour
{
    [SerializeField] public string charName;
    [SerializeField] public GameObject prefab;

    class CharactersBaker : Baker<CharactersAuthoring>
    {
        public override void Bake(CharactersAuthoring authoring)
        {
            // character entity
            var authoringEntity = GetEntity(authoring, TransformUsageFlags.None);
            var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);

            AddComponent(authoringEntity, new PrefabReferenceComponent { Value = prefabEntity });
            AddComponent(authoringEntity, new NameDataComponent{ Name = authoring.charName });
            AddComponent(authoringEntity, new TurnComponent{ IsCurrentActivePlayer = false });
            AddComponent(authoringEntity, new PrefabTag());
        }
    }
}

public struct PrefabReferenceComponent : IComponentData
{
    public Entity Value;
}

public struct NameDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct PrefabComponent : IComponentData
{
    public Entity prefab;
}

public struct TurnComponent : IComponentData
{
    public bool IsCurrentActivePlayer;
}

