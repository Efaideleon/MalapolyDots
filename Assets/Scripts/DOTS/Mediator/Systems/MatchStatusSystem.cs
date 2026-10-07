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
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PropertyTapHintSystem))]
    public partial class MatchStatusSystem : SystemBase
    {
        VisualElement root, turnCard, roundProgress, results, resultsCard;
        ScrollView standings;
        Label summary, hint, propertyTapHint, roundSummary, title, reason, eyebrow, returnStatus;
        readonly List<(string name, int id, int worth, bool outOfGame)> lastRows = new();
        bool renderedResults, displayedByRoundLimit;
        int displayedRound = -1, displayedLimit = -1, displayedWinner = -1;
        double nextResultsUpdate;

        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<ForegroundContainterComponent>();
            RequireForUpdate<GameScreenInitializedFlag>();
            RequireForUpdate<NetworkId>();
        }

        protected override void OnStartRunning()
        {
            if (resultsCard == null || !resultsCard.ClassListContains("is-disconnected")) return;
            root?.RemoveFromHierarchy();
            root = null;
        }

        protected override void OnUpdate()
        {
            var foreground = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>().Value;
            if (foreground == null) return;
            if (root == null || root.panel == null || root.parent != foreground)
            {
                root?.RemoveFromHierarchy();
                var asset = Resources.Load<VisualTreeAsset>("UI/MatchStatus");
                if (asset == null) return;
                root = asset.CloneTree();
                root.AddToClassList("match-status");
                root.pickingMode = PickingMode.Ignore;
                foreground.Add(root);
                turnCard = root.Q("turnCard");
                roundProgress = root.Q("roundProgress");
                results = root.Q("results");
                resultsCard = root.Q(className: "match-results-card");
                standings = root.Q<ScrollView>("standings");
                summary = root.Q<Label>("turnSummary");
                hint = root.Q<Label>("turnHint");
                propertyTapHint = root.Q<Label>("propertyTapHint");
                roundSummary = root.Q<Label>("roundSummary");
                title = root.Q<Label>("resultTitle");
                reason = root.Q<Label>("resultReason");
                eyebrow = root.Q<Label>("resultEyebrow");
                returnStatus = root.Q<Label>("returnStatus");
                summary.enableRichText = hint.enableRichText = propertyTapHint.enableRichText = title.enableRichText = false;
                root.Q<Button>("returnToMenu").clicked += () =>
                {
                    returnStatus.text = "Leaving the table…";
                    NetworkRequests.ReturnToMainMenu?.Invoke();
                };
                var statusLabel = returnStatus;
                statusLabel.schedule.Execute(() => statusLabel.text = NetworkRequests.MenuReturnStatus).Every(250);
                renderedResults = false;
                displayedRound = displayedLimit = displayedWinner = -1;
                lastRows.Clear();
            }
            returnStatus.text = NetworkRequests.MenuReturnStatus;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            int localId = SystemAPI.GetSingleton<NetworkId>().Value;
            if (game.State == GameState.GameOver)
            {
                if (renderedResults && SystemAPI.Time.ElapsedTime < nextResultsUpdate) return;
                nextResultsUpdate = SystemAPI.Time.ElapsedTime + 0.5;
                RenderResults(game, localId);
                return;
            }

            string activeName = "Waiting for players";
            Entity activePlayer = Entity.Null;
            bool isLocalTurn = false, eliminated = false;
            foreach (var (owner, bankrupt) in SystemAPI.Query<RefRO<GhostOwner>, RefRO<BankruptPlayer>>())
                if (owner.ValueRO.NetworkId == localId) eliminated = bankrupt.ValueRO.Value;
            if (SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active) &&
                EntityManager.HasComponent<NameComponent>(active.Entity) && EntityManager.HasComponent<GhostOwner>(active.Entity))
            {
                activePlayer = active.Entity;
                activeName = EntityManager.GetComponentData<NameComponent>(activePlayer).Value.ToString();
                int activeId = EntityManager.GetComponentData<GhostOwner>(activePlayer).NetworkId;
                if (SoloSession.IsAiId(activeId)) activeName += " (AI)";
                isLocalTurn = activeId == localId;
            }
            UpdateRounds(game);
            summary.text = eliminated ? "Spectating" : isLocalTurn ? "Your turn" : activeName + (activePlayer == Entity.Null ? "" : "'s turn");
            if (activePlayer == Entity.Null) hint.text = "The table is getting ready.";
            else if (game.State == GameState.Walking) hint.text = isLocalTurn ? "Moving around the board…" : activeName + " is moving…";
            else if (game.State == GameState.Landing) hint.text = isLocalTurn ? "Resolve this space, then end your turn." : "Waiting for " + activeName + " to finish their turn.";
            else hint.text = isLocalTurn ? "Roll the dice to start your move." : "Waiting for " + activeName + " to roll.";
            if (isLocalTurn && game.State == GameState.Rolling && EntityManager.HasComponent<JailState>(activePlayer) &&
                EntityManager.GetComponentData<JailState>(activePlayer).InJail)
                hint.text = "You are in jail. Resolve your jail turn to continue.";

            bool hasDebt = false;
            foreach (var (name, owner, money, bankruptcy) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostOwner>, RefRO<GhostMoneyComponet>, RefRO<BankruptPlayer>>())
                if (!bankruptcy.ValueRO.Value && money.ValueRO.Value < 0)
                {
                    bool localDebt = owner.ValueRO.NetworkId == localId;
                    summary.text = localDebt ? $"Raise ${-(long)money.ValueRO.Value:N0}" : name.ValueRO.Value + " is raising money";
                    hint.text = localDebt ? "Open Buy & Sell to sell assets and stay in the game." : "The table will continue once the debt is resolved.";
                    hasDebt = true;
                    break;
                }
            turnCard.EnableInClassList("is-local-turn", isLocalTurn && !eliminated);
            turnCard.EnableInClassList("is-debt", hasDebt);
            bool showTapHint = SystemAPI.TryGetSingleton<PropertyTapHintState>(out var tapHint) &&
                !tapHint.Learned && tapHint.CanPrompt && tapHint.Property != Entity.Null && !hasDebt &&
                EntityManager.HasComponent<NameComponent>(tapHint.Property);
            propertyTapHint.EnableInClassList("is-visible", showTapHint);
            if (showTapHint)
                propertyTapHint.text = "Tap " + EntityManager.GetComponentData<NameComponent>(tapHint.Property).Value + " to open its actions.";
        }

        void UpdateRounds(GameStateComponent game)
        {
            int limit = game.RoundLimit <= 0 ? 8 : game.RoundLimit;
            int round = Mathf.Clamp(game.CompletedRounds + 1, 1, limit);
            if (displayedRound == round && displayedLimit == limit) return;
            if (displayedLimit != limit)
            {
                roundProgress.Clear();
                for (int i = 0; i < limit; i++)
                {
                    var step = new VisualElement { pickingMode = PickingMode.Ignore };
                    step.AddToClassList("match-round-step");
                    roundProgress.Add(step);
                }
            }
            displayedRound = round;
            displayedLimit = limit;
            roundSummary.text = $"Round {round} / {limit}";
            for (int i = 0; i < roundProgress.childCount; i++)
            {
                roundProgress[i].EnableInClassList("is-complete", i < round - 1);
                roundProgress[i].EnableInClassList("is-current", i == round - 1);
            }
        }

        void RenderResults(GameStateComponent game, int localId)
        {
            bool firstRender = !renderedResults;
            renderedResults = true;
            turnCard.AddToClassList("is-hidden");
            results.AddToClassList("is-visible");
            if (firstRender)
            {
                root.BringToFront();
                root.Q<Button>("returnToMenu").Focus();
            }
            var rows = new List<(string name, int id, int worth, bool outOfGame)>();
            foreach (var (name, owner, bankrupt) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostOwner>, RefRO<BankruptPlayer>>())
                rows.Add((name.ValueRO.Value.ToString(), owner.ValueRO.NetworkId, bankrupt.ValueRO.FinalNetWorth, bankrupt.ValueRO.Value));
            rows.Sort((a, b) =>
            {
                int eliminatedOrder = a.outOfGame.CompareTo(b.outOfGame);
                if (eliminatedOrder != 0) return eliminatedOrder;
                int worthOrder = b.worth.CompareTo(a.worth);
                return worthOrder != 0 ? worthOrder : a.id.CompareTo(b.id);
            });
            string winner = rows.Find(x => x.id == game.WinnerNetworkId).name ?? "Winner";
            title.text = game.WinnerNetworkId == 0 ? "The match is a draw!" : game.WinnerNetworkId == localId ? "You won the table!" : $"{winner} wins!";
            reason.text = game.EndedByRoundLimit ? $"{(game.RoundLimit <= 0 ? 8 : game.RoundLimit)} rounds played. Highest net worth wins." : "The last player standing wins.";
            bool changed = firstRender || displayedWinner != game.WinnerNetworkId || displayedByRoundLimit != game.EndedByRoundLimit || rows.Count != lastRows.Count;
            if (!changed)
                for (int i = 0; i < rows.Count; i++)
                    if (rows[i] != lastRows[i]) { changed = true; break; }
            if (!changed) return;
            displayedWinner = game.WinnerNetworkId;
            displayedByRoundLimit = game.EndedByRoundLimit;
            lastRows.Clear();
            lastRows.AddRange(rows);
            var scrollOffset = standings.scrollOffset;
            standings.contentContainer.Clear();
            int rank = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (i == 0 || row.worth != rows[i - 1].worth) rank = i + 1;
                bool local = row.id == localId;
                bool winnerRow = !row.outOfGame && (row.id == game.WinnerNetworkId ||
                    (game.WinnerNetworkId == 0 && game.EndedByRoundLimit && rank == 1));
                var card = new VisualElement();
                card.AddToClassList("match-standing");
                card.EnableInClassList("is-winner", winnerRow);
                card.EnableInClassList("is-local", local);
                card.EnableInClassList("is-bankrupt", row.outOfGame);
                card.Add(StyledLabel(row.outOfGame ? "—" : rank.ToString(), "match-rank"));
                var player = new VisualElement();
                player.AddToClassList("match-standing-player");
                player.Add(StyledLabel(row.name, "match-player-name"));
                string badge = local ? "YOU" : SoloSession.IsAiId(row.id) ? "AI OPPONENT" : "PLAYER";
                if (winnerRow) badge = (game.WinnerNetworkId == 0 ? "JOINT LEADER" : "WINNER") + " · " + badge;
                player.Add(StyledLabel(badge, "match-player-badge"));
                card.Add(player);
                card.Add(StyledLabel(row.outOfGame ? "Bankrupt" : "$" + row.worth.ToString("N0"), "match-player-worth"));
                standings.Add(card);
            }
            standings.scrollOffset = scrollOffset;
        }

        static Label StyledLabel(string text, string className)
        {
            var label = new Label(text) { enableRichText = false };
            label.AddToClassList(className);
            return label;
        }

        protected override void OnStopRunning()
        {
            if (root == null || root.panel == null || renderedResults) return;
            turnCard.AddToClassList("is-hidden");
            eyebrow.text = "MALAPOLY / TABLE CLOSED";
            title.text = "Connection closed";
            reason.text = "The host left or the connection was lost. Return to the main menu to join another table.";
            resultsCard.AddToClassList("is-disconnected");
            standings.contentContainer.Clear();
            results.AddToClassList("is-visible");
            root.BringToFront();
            root.Q<Button>("returnToMenu").Focus();
        }

        protected override void OnDestroy() => root?.RemoveFromHierarchy();
    }
}
