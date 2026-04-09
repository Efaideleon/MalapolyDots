using Assets.Scripts.DOTS.DataComponents;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class SceneLoaderAuthoring : MonoBehaviour
    {
        public EntitySceneReference GameScene;
        public class SceneLoaderBaker : Baker<SceneLoaderAuthoring>
        {
            public override void Bake(SceneLoaderAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new SceneLoader
                {
                    SceneEntityReference = authoring.GameScene
                });
            }
        }
    }
}
