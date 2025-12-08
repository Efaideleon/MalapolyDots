using UnityEngine;
using DOTS.DataComponents;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "Property", menuName = "Scriptable Objects/Space/Property")]
    public class PropertySpaceData : SpaceData
    {
        public int price;
        public int[] rent;
        public int rentWithHotel;
        public PropertyColor Color;
    }
}
