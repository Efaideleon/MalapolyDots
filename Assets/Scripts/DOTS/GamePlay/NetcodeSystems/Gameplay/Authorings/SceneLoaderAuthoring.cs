using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class SceneLoaderAuthoring : MonoBehaviour
    {
        public UnityEditor.SceneAsset Scene;
        public class SceneLoaderBaker : Baker<SceneLoaderAuthoring>
        {
            public override void Bake(SceneLoaderAuthoring authoring)
            {
                var reference = new EntitySceneReference(authoring.Scene);
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SceneLoader
                {
                    SceneReference = reference
                });
            }
        }
    }

    public struct SceneLoader : IComponentData
    {
        public EntitySceneReference SceneReference;
    }
}
