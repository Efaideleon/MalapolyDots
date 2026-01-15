using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings
{
    public class PrepickedCharacterSpawnerAuthoring : MonoBehaviour
    {
        public GameObject GhostPrefab;
        public class PrepickedCharacterSpawnerBaker : Baker<PrepickedCharacterSpawnerAuthoring>
        {
            public override void Bake(PrepickedCharacterSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new PrepickedCharacterReference
                {
                    Prefab = GetEntity(authoring.GhostPrefab, TransformUsageFlags.None)
                });

            }
        }
    }

    public struct PrepickedCharacterReference : IComponentData
    {
        public Entity Prefab;
    }
}
