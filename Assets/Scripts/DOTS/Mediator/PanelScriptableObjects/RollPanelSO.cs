using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "RollPanel", menuName = "Scriptable Objects/PanelData/RollPanel")]
    public class RollPanelSO : ScriptableObject
    {
        public string RollAmount;

        public void OnEnable()
        {
            RollAmount = "";
        }
    }
}
