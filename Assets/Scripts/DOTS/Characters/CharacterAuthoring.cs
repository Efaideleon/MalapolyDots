using Unity.Entities;
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

            AddComponent(authoringEntity, new NameComponent{ Value = authoring.charName });
            AddComponent(authoringEntity, new PrefabTag());
            AddComponent(authoringEntity, new WayPointsBufferIndex{ Index = 0 } );
            AddComponent(authoringEntity, new CharacterFlag{});
            AddComponent(authoringEntity, new PlayerID { Value = 0 });
        }
    }
}

public struct PlayerID : IComponentData
{
    public int Value;
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
