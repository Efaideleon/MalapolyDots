using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleScreenCanvasAuthoring : MonoBehaviour
{
    [SerializeField] public UIDocument documentGO;

    public class TitleScreenBaker : Baker<TitleScreenCanvasAuthoring>
    {
        public override void Bake(TitleScreenCanvasAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponentObject(entity, new TitleScreenCanvasReference { uiDocumentGO = authoring.documentGO.gameObject });
        }
    }
}

public class TitleScreenCanvasReference : IComponentData
{
    public GameObject uiDocumentGO;
}
