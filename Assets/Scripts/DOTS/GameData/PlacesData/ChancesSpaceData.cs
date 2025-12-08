using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "Chance", menuName = "Scriptable Objects/Space/Chance")]
    public class ChancesSpaceData : SpaceData
    {
        public ChanceActionData[] chancesActionData;
    }

    [System.Serializable]
    public struct ChanceActionData
    {
        public int id;
        public string msg;
    }
}
