using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Authoring
{
    public class JoinByCodeUIAuthoring : MonoBehaviour
    {
        public UIDocument documentGO;

        public class JoinByCodeUIBaker : Baker<JoinByCodeUIAuthoring>
        {
            public override void Bake(JoinByCodeUIAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new JoinByCodeUIReference { uiDocumentGO = authoring.documentGO.gameObject });
            }
        }
    }

    public class JoinByCodeUIReference : IComponentData
    {
        public GameObject uiDocumentGO;
    }
}
