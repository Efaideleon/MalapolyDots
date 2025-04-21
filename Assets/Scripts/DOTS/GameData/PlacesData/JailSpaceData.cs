using UnityEngine;

namespace DOTS.GameData.PlacesData
{
    [CreateAssetMenu(fileName = "JailSpaceSO", menuName = "Scriptable Objects/JailSpaceSO")]
    public class JailSpaceData: ScriptableObject
    {
        public int id;
        public string Name;
        public int boardIndex;
    }
}
