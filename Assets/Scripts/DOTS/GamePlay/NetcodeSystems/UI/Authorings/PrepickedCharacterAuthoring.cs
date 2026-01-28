using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings
{
    public class PrepickedCharacterAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public CharactersEnum Character;

        public class PrepickedCharacterBaker : Baker<PrepickedCharacterAuthoring>
        {
            public override void Bake(PrepickedCharacterAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new PrepickedCharacter
                {
                    Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    PrePicked = false,
                    Character = authoring.Character,
                    Owner = default
                });
            }
        }
    }
}
