using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    [SerializeField] public string charName;

    class CharactersBaker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            // character entity
            var authoringEntity = GetEntity(authoring, TransformUsageFlags.None);

            AddComponent(authoringEntity, new NameDataComponent{ Value = authoring.charName });
            AddComponent(authoringEntity, new PlayerTurnComponent{ IsActive = false });
            AddComponent(authoringEntity, new PrefabTag());
            AddComponent(authoringEntity, new WayPointsBufferIndex{ Index = 0 } );
            AddComponent(authoringEntity, new CharacterFlag{});
        }
    }
}

public struct NameDataComponent : IComponentData
{
    public FixedString64Bytes Value;
}

public struct PlayerTurnComponent : IComponentData
{
    public bool IsActive;
}

public struct WayPointsBufferIndex : IComponentData
{
    public int Index;
}

public struct CharacterFlag : IComponentData
{}
