
using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "PurchaseHousePanel", menuName = "Scriptable Objects/PanelData/PurchaseHousePanel")]
    public class PurchaseHousePanelSO : ScriptableObject
    {
        public string PropertyName; 
        public string BuyingHouseCounter; 
        public string HousesOwnedCounter;
    }
}
