using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class SceneLoaderAuthoring : MonoBehaviour
    {
        public UnityEditor.SceneAsset GameScene;
        public class SceneLoaderBaker : Baker<SceneLoaderAuthoring>
        {
            public override void Bake(SceneLoaderAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var path = AssetDatabase.GetAssetPath(authoring.GameScene);
                var guid = AssetDatabase.GUIDFromAssetPath(path);

                AddComponent(entity, new SceneLoader
                {
                    GameSceneGuid = guid
                });
            }
        }
    }

    [GhostComponent]
    public struct SceneLoader : IComponentData
    {
        [GhostField]
        public Unity.Entities.Hash128 GameSceneGuid;
    }
}
