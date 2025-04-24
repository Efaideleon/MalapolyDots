using Unity.Entities;
using UnityEngine;

public class UISpritesAuthoring : MonoBehaviour
{
    [SerializeField] public Texture2D textureAtlas;

    public class SpritesBaker : Baker<UISpritesAuthoring>
    {
        public override void Bake(UISpritesAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponentObject(entity, new UISpriteComponent { Atlas = authoring.textureAtlas });
        }
    }

    public class UISpriteComponent : IComponentData
    {
        public Texture2D Atlas;
    }
}
