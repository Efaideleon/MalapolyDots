
using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "TaxPanel", menuName = "Scriptable Objects/PanelData/TaxPanel")]
    public class TaxPanelSO : ScriptableObject
    {
        public string AmountLabel; 
    }
}
