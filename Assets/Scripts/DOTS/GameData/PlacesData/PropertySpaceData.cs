using UnityEngine;
using DOTS.DataComponents;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "Property", menuName = "Scriptable Objects/Space/Property")]
    public class PropertySpaceData : SpaceData
    {
        public int price;
        [Min(0)] public int housePrice = 50;
        public int[] rent;
        public int rentWithHotel;
        public PropertyColor Color;
    }
}
