using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    [FormerlySerializedAs("NumberOfPlayers")] [SerializeField] public int numberOfPlayers;
}
