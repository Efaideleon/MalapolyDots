using UnityEngine;

[CreateAssetMenu(fileName = "TaxSpaceSO", menuName = "Scriptable Objects/TaxSpaceSO")]
public class TaxSpaceData: SpaceData
{
    [SerializeField] public float TaxAmount = 200;
}
