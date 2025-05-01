using Unity.Entities;
using UnityEngine;

namespace DOTS.UI.Utilities
{
    public class CanvasReferenceComponent : IComponentData
    {
        public GameObject uiDocumentGO;
        public Sprite[] spaceSprites;
        public Sprite[] characterSprites;
    }
}
