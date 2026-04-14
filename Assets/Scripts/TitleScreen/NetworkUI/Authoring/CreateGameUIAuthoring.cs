using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Authoring
{
    public class CreateGameUIAuthoring : MonoBehaviour
    {
        public UIDocument documentGO;

        public class CreateGameUIBaker : Baker<CreateGameUIAuthoring>
        {
            public override void Bake(CreateGameUIAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new CreateGameUIReference { uiDocumentGO = authoring.documentGO.gameObject });
            }
        }
    }

    public class CreateGameUIReference : IComponentData
    {
        public GameObject uiDocumentGO;
    }
}
