using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "TreasureCard", menuName = "Scriptable Objects/Space/TreasureCard")]
    public class TreasureCard : ScriptableObject
    {
        public int id;
        public string msg;
    }
}
