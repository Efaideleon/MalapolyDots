using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "Treasure", menuName = "Scriptable Objects/Space/Treasure")]
    public class TreasureSpaceData : SpaceData
    {
        public TreasureCard[] Cards;
    }
}
