using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "GoSpaceSO", menuName = "Scriptable Objects/GoSpaceSO")]
    public class GoSpaceData : ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
