using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Authoring
{
    public class EnterCodeUIAuthoring : MonoBehaviour
    {
        public UIDocument documentGO;

        public class EnterCodeUIBaker : Baker<EnterCodeUIAuthoring>
        {
            public override void Bake(EnterCodeUIAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new EnterCodeUIReference { uiDocumentGO = authoring.documentGO.gameObject });
            }
        }
    }

    public class EnterCodeUIReference : IComponentData
    {
        public GameObject uiDocumentGO;
    }
}
