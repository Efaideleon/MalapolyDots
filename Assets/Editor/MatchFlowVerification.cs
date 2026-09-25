using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;

public static class MatchFlowVerification
{
    [MenuItem("Tools/Malapoly/Finish Solo Verification Match")]
    static void FinishSoloMatch()
    {
        var world = ClientServerBootstrap.ServerWorld;
        if (!Application.isPlaying || world == null) return;
        var manager = world.EntityManager;
        using var connections = manager.CreateEntityQuery(typeof(NetworkStreamInGame));
        using var game = manager.CreateEntityQuery(typeof(GameStateComponent));
        using var round = manager.CreateEntityQuery(typeof(CurrentRound));
        if (connections.CalculateEntityCount() != 1 || game.CalculateEntityCount() != 1 || round.CalculateEntityCount() != 1)
        {
            Debug.LogWarning("Verification requires a running solo match; remote matches cannot be changed.");
            return;
        }
        var match = game.GetSingleton<GameStateComponent>();
        if (!match.AllPlacesInstantiated) return;
        manager.SetComponentData(round.GetSingletonEntity(), new CurrentRound { Value = match.RoundLimit == 0 ? 8 : match.RoundLimit });
    }
}
