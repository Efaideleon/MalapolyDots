using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "PurchasePropertyPanel", menuName = "Scriptable Objects/PanelData/PurchasePropertyPanel")]
    public class PurchasePropertyPanelSO : ScriptableObject
    {
        public string PriceLabel;
        public string NameLabel;
        public Sprite Image;
        // ADD for the image?

        public void OnEnable()
        {
            NameLabel = "Name of Property";
            PriceLabel = "Price";
        }
    }
}
