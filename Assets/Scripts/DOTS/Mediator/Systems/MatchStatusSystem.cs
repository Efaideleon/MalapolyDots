using System.Collections.Generic;
using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class MatchStatusSystem : SystemBase
    {
        VisualElement root, results, standings;
        Label summary, title, reason;
        bool renderedResults;
        double nextResultsUpdate;

        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<ForegroundContainterComponent>();
            RequireForUpdate<GameScreenInitializedFlag>();
            RequireForUpdate<NetworkId>();
        }

        protected override void OnUpdate()
        {
            var foreground = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>().Value;
            if (foreground == null) return;
            if (root == null || root.panel == null)
            {
                root?.RemoveFromHierarchy();
                var asset = Resources.Load<VisualTreeAsset>("UI/MatchStatus");
                if (asset == null) return;
                root = asset.CloneTree();
                root.AddToClassList("match-status");
                root.pickingMode = PickingMode.Ignore;
                foreground.Add(root);
                results = root.Q("results");
                standings = root.Q("standings");
                summary = root.Q<Label>("turnSummary");
                title = root.Q<Label>("resultTitle");
                reason = root.Q<Label>("resultReason");
                root.Q<Button>("returnToMenu").clicked += () =>
                {
                    root.Q<Label>("returnStatus").text = "Leaving the table…";
                    NetworkRequests.ReturnToMainMenu?.Invoke();
                };
                var returnStatus = root.Q<Label>("returnStatus");
                returnStatus.schedule.Execute(() => returnStatus.text = NetworkRequests.MenuReturnStatus).Every(250);
                renderedResults = false;
            }
            root.Q<Label>("returnStatus").text = NetworkRequests.MenuReturnStatus;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            int localId = SystemAPI.GetSingleton<NetworkId>().Value;
            string activeName = "Waiting for players";
            bool isLocalTurn = false, eliminated = false;
            foreach (var (name, owner, bankrupt) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostOwner>, RefRO<BankruptPlayer>>())
                if (owner.ValueRO.NetworkId == localId) eliminated = bankrupt.ValueRO.Value;
            if (SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active) &&
                EntityManager.HasComponent<NameComponent>(active.Entity) && EntityManager.HasComponent<GhostOwner>(active.Entity))
            {
                activeName = EntityManager.GetComponentData<NameComponent>(active.Entity).Value.ToString();
                isLocalTurn = EntityManager.GetComponentData<GhostOwner>(active.Entity).NetworkId == localId;
            }
            int limit = game.RoundLimit == 0 ? 8 : game.RoundLimit;
            summary.text = $"Round {System.Math.Min(game.CompletedRounds + 1, limit)} / {limit}   ·   " +
                (eliminated ? "You are bankrupt · Spectating" : isLocalTurn ? "Your turn" : $"{activeName}'s turn");
            if (game.State != GameState.GameOver || (renderedResults && SystemAPI.Time.ElapsedTime < nextResultsUpdate)) return;
            nextResultsUpdate = SystemAPI.Time.ElapsedTime + 0.5;
            renderedResults = true;
            results.AddToClassList("is-visible");
            root.BringToFront();
            var rows = new List<(string name, int id, int worth, bool outOfGame)>();
            foreach (var (name, owner, bankrupt) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostOwner>, RefRO<BankruptPlayer>>())
                rows.Add((name.ValueRO.Value.ToString(), owner.ValueRO.NetworkId, bankrupt.ValueRO.FinalNetWorth, bankrupt.ValueRO.Value));
            rows.Sort((a, b) => b.worth.CompareTo(a.worth));
            string winner = rows.Find(x => x.id == game.WinnerNetworkId).name ?? "Winner";
            title.text = game.WinnerNetworkId == 0 ? "The match is a draw" : game.WinnerNetworkId == localId ? "You won!" : $"{winner} wins!";
            reason.text = game.EndedByRoundLimit ? "Round limit reached. Highest net worth wins." : "The last player standing wins.";
            standings.Clear();
            foreach (var row in rows)
            {
                var label = new Label($"{row.name}{(row.id == localId ? " (you)" : "")}   ·   {(row.outOfGame ? "Bankrupt" : "$" + row.worth.ToString("N0"))}") { enableRichText = false };
                label.AddToClassList("match-standing");
                standings.Add(label);
            }
        }

        protected override void OnStopRunning()
        {
            if (root == null || root.panel == null) return;
            if (!renderedResults)
            {
                title.text = "Connection closed";
                reason.text = "The host left or the connection was lost. Return to the main menu to join another table.";
                standings.Clear();
                results.AddToClassList("is-visible");
                root.BringToFront();
            }
        }

        protected override void OnDestroy() => root?.RemoveFromHierarchy();
    }
}
