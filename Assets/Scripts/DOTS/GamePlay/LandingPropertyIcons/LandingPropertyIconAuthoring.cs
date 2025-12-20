using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.LandingPropertyIcons
{
    public class LandingPropertyIconAuthoring : MonoBehaviour
    {
        public GameObject IconPrefab;

        public class LandingPropertyIconBaker : Baker<LandingPropertyIconAuthoring>
        {
            public override void Bake(LandingPropertyIconAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new IconPrefabGameObject { IconPrefab = authoring.IconPrefab });
            }
        }
    }

    public class IconPrefabGameObject : IComponentData
    {
        public GameObject IconPrefab;
    }
}
