using DOTS.DataComponents;
using DOTS.GameData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters
{
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
                AddComponent(authoringEntity, new MoneyComponent{ Value = 500_000 });
                AddComponent(authoringEntity, new PrefabTag());
                AddComponent(authoringEntity, new PlayerWaypointIndex{ Value = 0 } );
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

    public struct PlayerWaypointIndex : IComponentData
    {
        public int Value;
    }

    public struct CharacterFlag : IComponentData
    {}
}
