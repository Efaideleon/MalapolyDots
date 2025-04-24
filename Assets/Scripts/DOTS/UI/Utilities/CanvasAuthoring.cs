using UnityEngine;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Utilities
{
    public class CanvasAuthoring : MonoBehaviour
    {
        [SerializeField] public UIDocument documentGO;
        [SerializeField] public Sprite[] spaceSprites;

        public class CanvasBaker : Baker<CanvasAuthoring>
        {
            public override void Bake(CanvasAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new CanvasReferenceComponent
                {
                    uiDocumentGO = authoring.documentGO.gameObject,
                    spaceSprites = authoring.spaceSprites
                });
            }
        }
    }
}
