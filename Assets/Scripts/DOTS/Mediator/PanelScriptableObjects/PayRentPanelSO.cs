using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "PayRentPanel", menuName = "Scriptable Objects/PanelData/PayRentPanel")]
    public class PayRentPanelSO : ScriptableObject
    {
        public string message;

        public void OnEnable()
        {
            message = "";
        }
    }
}
