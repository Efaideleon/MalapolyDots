using UnityEngine;

[CreateAssetMenu(fileName = "PropertySpaceSO", menuName = "Scriptable Objects/PropertySpaceSO")]
public class PropertySpaceData: ScriptableObject
{
    public int id;
    public string Name;
    public int boardIndex;
    public int price;
}
