using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "GoToJailSpaceSO", menuName = "Scriptable Objects/GoToJailSpaceSO")]
    public class GoToJailSpaceData: ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
