using Unity.Entities;
using UnityEditor;
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
                var entity = GetEntity(TransformUsageFlags.None);

                var path = AssetDatabase.GetAssetPath(authoring.Scene);
                var guid = AssetDatabase.GUIDFromAssetPath(path);

                AddComponent(entity, new SceneLoader
                {
                    SceneGUID = guid,
                });
            }
        }
    }

    public struct SceneLoader : IComponentData
    {
        public Unity.Entities.Hash128 SceneGUID;
    }
}
