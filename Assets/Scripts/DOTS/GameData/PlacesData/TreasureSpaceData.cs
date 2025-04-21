using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "TreasureSpaceSO", menuName = "Scriptable Objects/TreasureSpaceSO")]
    public class TreasureSpaceData: ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
