using UnityEngine;

[CreateAssetMenu(fileName = "ParkingSpaceSO", menuName = "Scriptable Objects/ParkingSpaceSO")]
public class ParkingSpaceData: ScriptableObject
{
    public int id;
    public string Name;
    public int boardIndex;
}
