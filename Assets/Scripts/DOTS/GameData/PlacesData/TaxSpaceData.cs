using UnityEngine;

[CreateAssetMenu(fileName = "TaxSpaceSO", menuName = "Scriptable Objects/TaxSpaceSO")]
public class TaxSpaceData: ScriptableObject
{
    public int id;
    public string Name;
    public int boardIndex;
    public float TaxAmount = 200;
}
