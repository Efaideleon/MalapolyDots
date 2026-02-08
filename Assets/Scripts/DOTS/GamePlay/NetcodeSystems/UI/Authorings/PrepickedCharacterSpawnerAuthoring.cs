using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings
{
    public class PrepickedCharacterSpawnerAuthoring : MonoBehaviour
    {
        public GameObject[] GhostPrefab;
        public class PrepickedCharacterSpawnerBaker : Baker<PrepickedCharacterSpawnerAuthoring>
        {
            public override void Bake(PrepickedCharacterSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                var buffer = AddBuffer<PrepickedCharacterReference>(entity);

                foreach (var prefab in authoring.GhostPrefab)
                {
                    buffer.Add(new PrepickedCharacterReference { Prefab = GetEntity(prefab, TransformUsageFlags.Dynamic) });
                }
            }
        }
    }

    public struct PrepickedCharacterReference : IBufferElementData
    {
        public Entity Prefab;
    }
}
