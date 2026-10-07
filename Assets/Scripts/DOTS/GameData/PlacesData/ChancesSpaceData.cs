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
        public int amount;
        public DOTS.DataComponents.ChanceEffect effect;
        public int hotelAmount;
        // Travel destinations use the runtime board's zero-based indices.
        public int targetBoardIndex;
    }
}
