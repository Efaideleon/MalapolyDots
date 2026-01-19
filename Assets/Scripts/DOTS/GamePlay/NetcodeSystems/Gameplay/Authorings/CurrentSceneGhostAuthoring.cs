using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class CurrentSceneGhostAuthoring : MonoBehaviour
    {
        public class CurrentSceneGhostBaker : Baker<CurrentSceneGhostAuthoring>
        {
            public override void Bake(CurrentSceneGhostAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentSharedScene { SceneGUID = default });
            }
        }
    }

    [GhostComponent]
    public struct CurrentSharedScene : IComponentData
    {
        [GhostField]
        public Unity.Entities.Hash128 SceneGUID;
    }
}
