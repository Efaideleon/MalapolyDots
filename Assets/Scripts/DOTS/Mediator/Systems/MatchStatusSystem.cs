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
        struct OwnedPropertyResult
        {
            public string Name;
            public int Price, Buildings, Order;
            public PropertyColor Color;
        }

        VisualElement root, turnCard, roundProgress, results, resultsCard;
        ScrollView standings;
        Label summary, hint, propertyTapHint, roundSummary, title, reason, eyebrow, returnStatus;
        readonly List<(string name, int id, int worth, int cash, int properties, int buildings, bool outOfGame, string propertySignature)> lastRows = new();
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
                hint.text = "You are still in jail. Use a Get Out of Jail Free card if you have one, or end your turn.";

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
            var ownedProperties = GetOwnedProperties();
            var rows = new List<(string name, int id, int worth, int cash, int properties, int buildings, bool outOfGame, string propertySignature)>();
            foreach (var (name, owner, bankrupt) in SystemAPI.Query<RefRO<NameComponent>, RefRO<GhostOwner>, RefRO<BankruptPlayer>>())
            {
                int playerId = owner.ValueRO.NetworkId;
                ownedProperties.TryGetValue(playerId, out var properties);
                rows.Add((name.ValueRO.Value.ToString(), playerId, bankrupt.ValueRO.FinalNetWorth,
                    bankrupt.ValueRO.FinalCashWorth, bankrupt.ValueRO.FinalPropertyWorth,
                    bankrupt.ValueRO.FinalBuildingWorth, bankrupt.ValueRO.Value, PropertySignature(properties)));
            }
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
                ownedProperties.TryGetValue(row.id, out var properties);
                properties ??= new List<OwnedPropertyResult>();
                var card = new VisualElement();
                card.AddToClassList("match-standing");
                card.EnableInClassList("is-winner", winnerRow);
                card.EnableInClassList("is-local", local);
                card.EnableInClassList("is-bankrupt", row.outOfGame);
                var header = new VisualElement();
                header.AddToClassList("match-standing-header");
                header.Add(StyledLabel(row.outOfGame ? "—" : rank.ToString(), "match-rank"));
                var player = new VisualElement();
                player.AddToClassList("match-standing-player");
                player.Add(StyledLabel(row.name, "match-player-name"));
                string badge = local ? "YOU" : SoloSession.IsAiId(row.id) ? "AI OPPONENT" : "PLAYER";
                if (winnerRow) badge = (game.WinnerNetworkId == 0 ? "JOINT LEADER" : "WINNER") + " · " + badge;
                player.Add(StyledLabel(badge, "match-player-badge"));
                header.Add(player);
                header.Add(StyledLabel(row.outOfGame ? "BANKRUPT · $0" : "$" + row.worth.ToString("N0"), "match-player-worth"));
                card.Add(header);

                if (row.outOfGame)
                {
                    card.Add(StyledLabel("Bankrupt players score $0. Deeds below show any assets still assigned to them.", "match-bankrupt-note"));
                }
                else
                {
                    var breakdown = new VisualElement();
                    breakdown.AddToClassList("match-worth-breakdown");
                    breakdown.Add(CreateWorthStat("CASH", "$", row.cash, "cash"));
                    breakdown.Add(CreateWorthStat($"DEEDS · {properties.Count}", "D", row.properties, "property"));
                    breakdown.Add(CreateWorthStat("BUILDINGS", "B", row.buildings, "building"));
                    card.Add(breakdown);
                }
                card.Add(CreatePropertyPortfolio(properties));
                standings.Add(card);
            }
            standings.scrollOffset = scrollOffset;
        }

        Dictionary<int, List<OwnedPropertyResult>> GetOwnedProperties()
        {
            var result = new Dictionary<int, List<OwnedPropertyResult>>();
            foreach (var (propertyOwner, price, entity) in SystemAPI.Query<RefRO<OwnerByEntityComponent>, RefRO<GhostPriceComponent>>().WithEntityAccess())
            {
                var playerEntity = propertyOwner.ValueRO.Entity;
                if (playerEntity == Entity.Null || !EntityManager.Exists(playerEntity) ||
                    !EntityManager.HasComponent<GhostOwner>(playerEntity) ||
                    !EntityManager.HasComponent<NameComponent>(entity)) continue;

                int playerId = EntityManager.GetComponentData<GhostOwner>(playerEntity).NetworkId;
                if (!result.TryGetValue(playerId, out var properties))
                {
                    properties = new List<OwnedPropertyResult>();
                    result.Add(playerId, properties);
                }
                int buildings = EntityManager.HasComponent<HouseCount>(entity)
                    ? Mathf.Clamp(EntityManager.GetComponentData<HouseCount>(entity).Value, 0, 5) : 0;
                int order = EntityManager.HasComponent<SpaceIDComponent>(entity)
                    ? EntityManager.GetComponentData<SpaceIDComponent>(entity).Value : int.MaxValue;
                var color = EntityManager.HasComponent<ColorCodeComponent>(entity)
                    ? EntityManager.GetComponentData<ColorCodeComponent>(entity).Value : PropertyColor.None;
                properties.Add(new OwnedPropertyResult
                {
                    Name = EntityManager.GetComponentData<NameComponent>(entity).Value.ToString(),
                    Price = Mathf.Max(0, price.ValueRO.Value),
                    Buildings = buildings,
                    Order = order,
                    Color = color
                });
            }
            foreach (var properties in result.Values)
                properties.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : string.Compare(a.Name, b.Name, System.StringComparison.Ordinal));
            return result;
        }

        static string PropertySignature(List<OwnedPropertyResult> properties)
        {
            if (properties == null || properties.Count == 0) return string.Empty;
            var signature = new System.Text.StringBuilder();
            foreach (var property in properties)
                signature.Append(property.Order).Append(':').Append(property.Name).Append(':').Append(property.Price)
                    .Append(':').Append(property.Buildings).Append(':').Append((int)property.Color).Append('|');
            return signature.ToString();
        }

        static VisualElement CreateWorthStat(string label, string symbol, int amount, string type)
        {
            var stat = new VisualElement();
            stat.AddToClassList("match-worth-stat");
            stat.AddToClassList("match-worth-stat-" + type);
            var icon = StyledLabel(symbol, "match-worth-stat-icon");
            stat.Add(icon);
            var text = new VisualElement();
            text.AddToClassList("match-worth-stat-text");
            text.Add(StyledLabel(label, "match-worth-stat-label"));
            text.Add(StyledLabel("$" + amount.ToString("N0"), "match-worth-stat-value"));
            stat.Add(text);
            return stat;
        }

        static VisualElement CreatePropertyPortfolio(List<OwnedPropertyResult> properties)
        {
            var portfolio = new VisualElement();
            portfolio.AddToClassList("match-property-portfolio");
            var heading = new VisualElement();
            heading.AddToClassList("match-property-portfolio-heading");
            heading.Add(StyledLabel("PROPERTY PORTFOLIO", "match-property-portfolio-title"));
            heading.Add(StyledLabel(properties.Count.ToString("00") + " DEEDS", "match-property-portfolio-count"));
            portfolio.Add(heading);

            if (properties.Count == 0)
            {
                portfolio.Add(StyledLabel("No properties owned", "match-empty-portfolio"));
                return portfolio;
            }
            var tiles = new VisualElement();
            tiles.AddToClassList("match-property-tiles");
            foreach (var property in properties)
            {
                var deed = new VisualElement();
                deed.AddToClassList("match-deed-card");
                var colorBand = new VisualElement();
                colorBand.AddToClassList("match-deed-color-band");
                colorBand.AddToClassList("match-deed-color-" + PropertyColorClass(property.Color));
                deed.Add(colorBand);
                var deedHeading = new VisualElement();
                deedHeading.AddToClassList("match-deed-heading");
                deedHeading.Add(StyledLabel(property.Name, "match-deed-name"));
                deedHeading.Add(StyledLabel("$" + property.Price.ToString("N0"), "match-deed-price"));
                deed.Add(deedHeading);

                var buildings = new VisualElement();
                buildings.AddToClassList("match-deed-buildings");
                buildings.Add(StyledLabel(property.Buildings == 5 ? "HOTEL" : property.Buildings == 1 ? "1 HOUSE" : property.Buildings > 1 ? property.Buildings + " HOUSES" : "NO BUILDINGS", "match-deed-building-label"));
                var pips = new VisualElement();
                pips.AddToClassList("match-building-pips");
                for (int i = 0; i < 5; i++)
                {
                    var pip = new VisualElement();
                    pip.AddToClassList("match-building-pip");
                    pip.EnableInClassList("is-built", i < property.Buildings);
                    pips.Add(pip);
                }
                buildings.Add(pips);
                deed.Add(buildings);
                tiles.Add(deed);
            }
            portfolio.Add(tiles);
            return portfolio;
        }

        static string PropertyColorClass(PropertyColor color) => color switch
        {
            PropertyColor.White => "white",
            PropertyColor.Brown => "brown",
            PropertyColor.LightBlue => "light-blue",
            PropertyColor.Purple => "purple",
            PropertyColor.Orange => "orange",
            PropertyColor.Red => "red",
            PropertyColor.Yellow => "yellow",
            PropertyColor.Green => "green",
            PropertyColor.Blue => "blue",
            _ => "none"
        };

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
