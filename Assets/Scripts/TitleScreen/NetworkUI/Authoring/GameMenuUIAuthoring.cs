using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Authoring
{
    public class GameMenuUIAuthoring : MonoBehaviour
    {
        public UIDocument documentGO;

        public class GameMenuUIBaker : Baker<GameMenuUIAuthoring>
        {
            public override void Bake(GameMenuUIAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new NetworkGameMenuReference { uiDocumentGO = authoring.documentGO.gameObject });
            }
        }
    }

    public class NetworkGameMenuReference : IComponentData
    {
        public GameObject uiDocumentGO;
    }
}
