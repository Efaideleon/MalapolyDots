using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "ChancesSpaceDataSO", menuName = "Scriptable Objects/ChancesSpaceDataSO")]
    public class ChancesSpaceData : ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
