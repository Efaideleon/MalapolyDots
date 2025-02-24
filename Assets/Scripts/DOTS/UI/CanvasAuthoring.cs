using UnityEngine;
using Unity.Entities;
using UnityEngine.UIElements;

public class CanvasAuthoring : MonoBehaviour
{
    [SerializeField] public UIDocument documentGO;

    public class CanvasBaker : Baker<CanvasAuthoring>
    {
        public override void Bake(CanvasAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponentObject(entity, new CanvasReferenceComponent
            {
                uiDocumentGO = authoring.documentGO.gameObject
            });
        }
    }
}
