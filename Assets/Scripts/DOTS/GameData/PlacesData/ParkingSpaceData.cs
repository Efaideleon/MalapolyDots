using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "ParkingSpaceSO", menuName = "Scriptable Objects/ParkingSpaceSO")]
    public class ParkingSpaceData: ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
