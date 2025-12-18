using UnityEngine;

namespace DOTS.Mediator.PanelScriptableObjects
{
    [CreateAssetMenu(fileName = "TreasurePanel", menuName = "Scriptable Objects/PanelData/TreasurePanel")]
    public class TreasurePanelSO : ScriptableObject
    {
        public string TitleLabel;

        public void OnEnable()
        {
            TitleLabel = "";
        }
    }
}
