
using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "PurchasePropertyPanel", menuName = "Scriptable Objects/PanelData/PurchasePropertyPanel")]
    public class PurchasePropertyPanelSO : ScriptableObject
    {
        public string PriceLabel {get; private set; }
        public string NameLabel {get; private set; }
        // ADD for the image?
    }
}
