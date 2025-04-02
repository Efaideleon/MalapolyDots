using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    [SerializeField] public int numberOfPlayers;
    [SerializeField] public int numberOfRounds;
    [SerializeField] public List<string> charactersSelected;

    public void Reset()
    {
        numberOfPlayers = 0;
        numberOfRounds = 0;
        charactersSelected = new List<string>();
    }
}
