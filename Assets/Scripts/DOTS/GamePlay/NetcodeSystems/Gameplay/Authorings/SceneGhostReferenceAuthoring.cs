using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class SceneGhostReferenceAuthoring : MonoBehaviour
    {
        public GameObject Prefab;

        public class SceneGhostReferenceAuthoringBaker : Baker<SceneGhostReferenceAuthoring>
        {
            public override void Bake(SceneGhostReferenceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentSceneGhostReference { entity = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct CurrentSceneGhostReference : IComponentData
    {
        public Entity entity;
    }
}
