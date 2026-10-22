using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MatchStatusSystem))]
    public partial class CardHandPresentationSystem : SystemBase
    {
        VisualElement root, overlay, cardsPanel, cardsList, modalShade;
        VisualElement monopolyChoicePanel, monopolyChoiceList;
        Button cardsButton, closeButton, discardNewButton, firstCardButton;
        Button cancelMonopolyChoice;
        Label cardsCount, panelCount;
        bool inventoryOpen, wasPendingChoice, wasPanelOpen;
        string displayedHand = string.Empty;
        string renderedMonopolyChoices = string.Empty;
        int selectedMonopolySlot = -1;
        List<MonopolyOption> currentMonopolyOptions = new List<MonopolyOption>();

        struct MonopolyOption
        {
            public PropertyColor Color;
            public int Cost;
        }

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
            if (root == null || root.panel == null || root.parent != foreground)
            {
                UnityEngine.Debug.Log($"[CardHandPresentationSystem] | are setting this up?");
                root?.RemoveFromHierarchy();
                var asset = Resources.Load<VisualTreeAsset>("UI/CardHand");
                if (asset == null) return;
                root = asset.CloneTree();
                // CloneTree() returns a wrapper whose absolutely positioned children do not
                // contribute to its height. Make that wrapper cover the foreground explicitly,
                // while letting clicks pass through it to the existing game UI.
                root.pickingMode = PickingMode.Ignore;
                root.style.position = Position.Absolute;
                root.style.left = 0;
                root.style.right = 0;
                root.style.top = 0;
                root.style.bottom = 0;
                foreground.Add(root);
                overlay = root.Q<VisualElement>("cardHandRoot");
                modalShade = root.Q<VisualElement>("cardsModalShade");
                cardsButton = root.Q<Button>("cardsButton");
                cardsPanel = root.Q("cardsPanel");
                cardsList = root.Q<VisualElement>("cardsList");
                monopolyChoicePanel = root.Q<VisualElement>("monopolyChoicePanel");
                monopolyChoiceList = root.Q<VisualElement>("monopolyChoiceList");
                cancelMonopolyChoice = root.Q<Button>("cancelMonopolyChoice");
                cardsCount = root.Q<Label>("cardsCount");
                panelCount = root.Q<Label>("cardsPanelCount");
                discardNewButton = root.Q<Button>("discardNewCard");
                closeButton = root.Q<Button>("closeCardsButton");
                cardsButton.RegisterCallback<MouseUpEvent>(HandleCardsButtonMouseUp, TrickleDown.TrickleDown);
                closeButton.clicked += () => SetInventoryOpen(false);
                cancelMonopolyChoice.clicked += HideMonopolyChoice;
                discardNewButton.clicked += () => SendOverflowChoice(CardInventoryRules.DiscardNewCard);
                displayedHand = string.Empty;
                wasPendingChoice = false;
                wasPanelOpen = false;
            }

            var game = SystemAPI.GetSingleton<GameStateComponent>();
            bool hasLocalPlayer = TryGetLocalPlayer(SystemAPI.GetSingleton<NetworkId>().Value,
                out var localPlayer, out var inventory);
            bool gameInProgress = game.State != GameState.GameOver;
            root.Q<Button>("cardsButton").EnableInClassList("is-hidden", !hasLocalPlayer || !gameInProgress);
            if (!hasLocalPlayer || !gameInProgress)
            {
                UnityEngine.Debug.Log($"[CardHandPresentationSystem] | closing the card inventory.");
                overlay?.RemoveFromClassList("is-open");
                modalShade?.RemoveFromClassList("is-active");
                closeButton?.RemoveFromClassList("is-hidden");
                if (cardsPanel != null) cardsPanel.style.display = DisplayStyle.None;
                inventoryOpen = false;
                wasPendingChoice = false;
                wasPanelOpen = false;
                return;
            }

            bool isLocalTurn = SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active) && active.Entity == localPlayer;
            bool isInJail = EntityManager.HasComponent<JailState>(localPlayer) &&
                EntityManager.GetComponentData<JailState>(localPlayer).InJail;
            bool canUseJailCard = isLocalTurn && isInJail && game.State == GameState.Rolling && !inventory.HasPendingChoice;
            bool isEliminated = EntityManager.HasComponent<BankruptPlayer>(localPlayer) &&
                EntityManager.GetComponentData<BankruptPlayer>(localPlayer).Value;
            canUseJailCard &= !isEliminated;
            var monopolyOptions = isLocalTurn && game.State == GameState.Rolling && !inventory.HasPendingChoice && !isEliminated
                ? GetAvailableMonopolyOptions(localPlayer)
                : new List<MonopolyOption>();
            bool canUseMonopolyCard = monopolyOptions.Count > 0;
            currentMonopolyOptions = monopolyOptions;

            cardsCount.text = $"{inventory.Count} / {CardInventoryRules.Capacity}";
            panelCount.text = $"{inventory.Count} / {CardInventoryRules.Capacity} CARDS";
            bool focusCardChoice = inventory.HasPendingChoice && !wasPendingChoice;
            wasPendingChoice = inventory.HasPendingChoice;
            bool shouldShowCardsPanel = inventoryOpen || inventory.HasPendingChoice;
            bool focusPanel = shouldShowCardsPanel && !wasPanelOpen;
            wasPanelOpen = shouldShowCardsPanel;
            overlay.EnableInClassList("is-open", shouldShowCardsPanel);
            modalShade.EnableInClassList("is-active", shouldShowCardsPanel);
            closeButton.EnableInClassList("is-hidden", inventory.HasPendingChoice);

            cardsPanel.style.display = shouldShowCardsPanel ? DisplayStyle.Flex : DisplayStyle.None;
            if (shouldShowCardsPanel) root.BringToFront();
            discardNewButton.EnableInClassList("is-visible", inventory.HasPendingChoice);

            string signature = BuildSignature(inventory, canUseJailCard, canUseMonopolyCard, isLocalTurn, isInJail, game.State, monopolyOptions);
            if (signature != displayedHand)
            {
                displayedHand = signature;
                RenderCards(inventory, canUseJailCard, canUseMonopolyCard, isLocalTurn, isInJail);
            }
            if (selectedMonopolySlot >= 0) RenderMonopolyChoices((byte)selectedMonopolySlot, monopolyOptions);
            if (focusCardChoice) firstCardButton?.Focus();
            else if (focusPanel) cardsPanel.Focus();
        }

        bool TryGetLocalPlayer(int localId, out Entity player, out PlayerCardInventory inventory)
        {
            player = Entity.Null;
            inventory = default;
            foreach (var (cards, owner, entity) in SystemAPI.Query<RefRO<PlayerCardInventory>, RefRO<GhostOwner>>().WithEntityAccess())
            {
                if (owner.ValueRO.NetworkId != localId) continue;
                player = entity;
                inventory = cards.ValueRO;
                return true;
            }
            return false;
        }

        static string BuildSignature(PlayerCardInventory hand, bool canUseJail, bool canUseMonopoly,
            bool isLocalTurn, bool inJail, GameState state, List<MonopolyOption> options)
        {
            var signature = new System.Text.StringBuilder(64);
            signature.Append(hand.Count).Append('|').Append(hand.HasPendingChoice).Append('|').Append(hand.PendingCard)
                .Append('|').Append(canUseJail).Append('|').Append(canUseMonopoly).Append('|')
                .Append(isLocalTurn).Append('|').Append(inJail).Append('|').Append((int)state);
            for (int i = 0; i < hand.Count; i++) signature.Append('|').Append((byte)CardInventoryRules.GetCard(hand, i));
            foreach (var option in options) signature.Append('|').Append((int)option.Color).Append(':').Append(option.Cost);
            return signature.ToString();
        }

        List<MonopolyOption> GetAvailableMonopolyOptions(Entity player)
        {
            var options = new List<MonopolyOption>();
            for (var color = PropertyColor.Brown; color <= PropertyColor.Blue; color++)
                if (ChanceCardRules.TryGetMonopolyPurchaseCost(EntityManager, player, color, out var cost))
                    options.Add(new MonopolyOption { Color = color, Cost = cost });
            return options;
        }

        void RenderCards(PlayerCardInventory hand, bool canUseJailCard, bool canUseMonopolyCard,
            bool isLocalTurn, bool isInJail)
        {
            firstCardButton = null;
            cardsList.Clear();
            if (hand.Count == 0)
            {
                var empty = new Label("Your card slots are waiting for a lucky draw.");
                empty.AddToClassList("cards-empty");
                cardsList.Add(empty);
                return;
            }

            for (int i = 0; i < hand.Count; i++)
            {
                int slot = i;
                var cardType = CardInventoryRules.GetCard(hand, slot);
                bool canUse = cardType == PlayerCardType.GetOutOfJailFree ? canUseJailCard :
                    cardType == PlayerCardType.Monopoly && canUseMonopolyCard;
                var card = new Button();
                card.AddToClassList("inventory-card");
                card.AddToClassList($"inventory-card-fan-{i}");
                card.clicked += () => HandleCardClick(card, (byte)slot, hand.HasPendingChoice, canUse, cardType);
                if (firstCardButton == null) firstCardButton = card;
                var face = new VisualElement();
                face.AddToClassList("inventory-card-visual");
                var topline = new VisualElement();
                topline.AddToClassList("inventory-card-topline");
                topline.Add(StyledLabel(hand.HasPendingChoice ? "PICK TO DISCARD" : canUse ? "PLAYABLE NOW" : "CHANCE · SPECIAL", "inventory-card-ribbon"));
                topline.Add(StyledLabel($"SLOT {slot + 1}", "inventory-card-slot"));
                face.Add(topline);
                face.Add(StyledLabel(CardTitle(cardType), "inventory-card-title"));
                face.Add(StyledLabel(CardDescription(cardType), "inventory-card-description"));
                face.Add(StyledLabel(hand.HasPendingChoice ? "CLICK TO DISCARD" : canUse ?
                    cardType == PlayerCardType.Monopoly ? "CLICK TO CHOOSE A SET" : "CLICK TO PLAY" : CardCondition(cardType),
                    "inventory-card-condition"));
                card.Add(face);
                cardsList.Add(card);
            }
        }

        void HandleCardClick(Button card, byte slot, bool pendingChoice, bool canUse, PlayerCardType cardType)
        {
            if (card.ClassListContains("is-playing")) return;

            if (pendingChoice)
            {
                SendOverflowChoice(slot);
                return;
            }

            if (canUse)
            {
                if (cardType == PlayerCardType.Monopoly)
                {
                    selectedMonopolySlot = slot;
                    RenderMonopolyChoices(slot, currentMonopolyOptions);
                    return;
                }
                card.AddToClassList("is-playing");
                root.schedule.Execute(() => SendUseRequest(slot, -1)).ExecuteLater(240);
                return;
            }

            // Cards that cannot be played in the current state are informational only.
            // Leave the hand and its visual state unchanged when one is clicked.
        }

        void RenderMonopolyChoices(byte slot, List<MonopolyOption> options)
        {
            if (monopolyChoiceList == null || monopolyChoicePanel == null) return;
            var signature = new System.Text.StringBuilder().Append(slot);
            foreach (var option in options) signature.Append('|').Append((int)option.Color).Append(':').Append(option.Cost);
            string nextSignature = signature.ToString();
            if (nextSignature == renderedMonopolyChoices) return;
            renderedMonopolyChoices = nextSignature;
            monopolyChoiceList.Clear();
            foreach (var option in options)
            {
                int color = (int)option.Color;
                int capturedSlot = slot;
                var button = new Button { text = $"{option.Color} · Q{option.Cost:N0}" };
                button.AddToClassList("monopoly-choice-button");
                button.clicked += () =>
                {
                    HideMonopolyChoice();
                    SendUseRequest((byte)capturedSlot, color);
                };
                monopolyChoiceList.Add(button);
            }
            monopolyChoicePanel.EnableInClassList("is-visible", options.Count > 0);
            if (options.Count == 0) selectedMonopolySlot = -1;
        }

        void HideMonopolyChoice()
        {
            selectedMonopolySlot = -1;
            renderedMonopolyChoices = string.Empty;
            monopolyChoicePanel?.RemoveFromClassList("is-visible");
        }

        void SetInventoryOpen(bool open)
        {
            inventoryOpen = open;
            if (open)
            {
                overlay.AddToClassList("is-open");
                cardsPanel.style.display = DisplayStyle.Flex;
                root.BringToFront();
                cardsPanel.BringToFront();
                UnityEngine.Debug.Log($"[CardHandPresentationSystem] | cardsPanel.resolvedStyle.display {cardsPanel.resolvedStyle.display}.");
                UnityEngine.Debug.Log($"[CardHandPresentationSystem] | cardsPanel.worldBound {cardsPanel.worldBound}.");
                cardsPanel.schedule.Execute(() =>
                {
                    UnityEngine.Debug.Log(
                        $"parent={root.parent?.worldBound}, root={root.worldBound}, " +
                        $"overlay={overlay.worldBound}, button={cardsButton.worldBound}, " +
                        $"panel={cardsPanel.worldBound}"
                    );
                }).ExecuteLater(100);
            }
            else
            {
                HideMonopolyChoice();
                overlay.RemoveFromClassList("is-open");
                cardsPanel.style.display = DisplayStyle.None;
            }
        }

        void HandleCardsButtonMouseUp(MouseUpEvent evt)
        {
            if (evt.button != 0) return;
            SetInventoryOpen(true);
        }

        void SendUseRequest(byte slot, int color)
        {
            var request = EntityManager.CreateEntity();
            EntityManager.AddComponentData(request, new UsePlayerCardRpc { Slot = slot, Color = color });
            EntityManager.AddComponentData(request, new SendRpcCommandRequest());
        }

        void SendOverflowChoice(byte discardSlot)
        {
            var request = EntityManager.CreateEntity();
            EntityManager.AddComponentData(request, new ResolveCardOverflowRpc { DiscardSlot = discardSlot });
            EntityManager.AddComponentData(request, new SendRpcCommandRequest());
        }

        static string UseButtonText(PlayerCardType card, bool canUse, bool isLocalTurn, bool isInJail)
        {
            if (card == PlayerCardType.Monopoly) return canUse ? "CHOOSE A SET" : "USE WHEN A SET IS AVAILABLE";
            if (card != PlayerCardType.GetOutOfJailFree) return "EFFECT NOT READY";
            if (canUse) return "USE CARD";
            if (!isInJail) return "USE WHEN IN JAIL";
            return isLocalTurn ? "WAIT FOR YOUR TURN" : "USE ON YOUR TURN";
        }

        static string CardTitle(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "GET OUT OF JAIL FREE",
            PlayerCardType.Monopoly => "MONOPOLY",
            _ => "MYSTERY CARD"
        };

        static string CardDescription(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "Keep this pass in your hand. Play it to leave Jail without spending a turn or rolling for doubles.",
            PlayerCardType.Monopoly => "Keep this card. Play it on your turn to buy the remaining deeds in an eligible color set.",
            _ => "This card's effect is not available yet."
        };

        static string CardCondition(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "PLAYABLE WHILE YOU ARE IN JAIL",
            PlayerCardType.Monopoly => "PLAYABLE ON YOUR TURN WHEN YOU CAN AFFORD A SET",
            _ => "NOT CURRENTLY PLAYABLE"
        };

        static Label StyledLabel(string text, string className)
        {
            var label = new Label(text) { enableRichText = false };
            label.AddToClassList(className);
            return label;
        }

        protected override void OnDestroy() => root?.RemoveFromHierarchy();
    }
}
