using UnityEngine;

[CreateAssetMenu(fileName = "BuyableSpaceSO", menuName = "Scriptable Objects/BuyableSpaceSO")]
public class PropertySpaceData: ScriptableObject
{
    public int id;
    public string Name;
    public int boardIndex;
    public float price;

}
