using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MatchStatusSystem))]
    public partial class CardHandPresentationSystem : SystemBase
    {
        VisualElement root, overlay, cardsPanel, cardsList, modalShade;
        Button cardsButton, closeButton, discardNewButton, firstCardButton;
        Label cardsCount, panelCount;
        bool inventoryOpen, wasPendingChoice, wasPanelOpen;
        string displayedHand = string.Empty;

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
                cardsCount = root.Q<Label>("cardsCount");
                panelCount = root.Q<Label>("cardsPanelCount");
                discardNewButton = root.Q<Button>("discardNewCard");
                closeButton = root.Q<Button>("closeCardsButton");
                cardsButton.RegisterCallback<MouseUpEvent>(HandleCardsButtonMouseUp, TrickleDown.TrickleDown);
                closeButton.clicked += () => SetInventoryOpen(false);
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

            string signature = BuildSignature(inventory, canUseJailCard, isLocalTurn, isInJail, game.State);
            if (signature != displayedHand)
            {
                displayedHand = signature;
                RenderCards(inventory, canUseJailCard, isLocalTurn, isInJail);
            }
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

        static string BuildSignature(PlayerCardInventory hand, bool canUse, bool isLocalTurn, bool inJail, GameState state)
        {
            var signature = new System.Text.StringBuilder(64);
            signature.Append(hand.Count).Append('|').Append(hand.HasPendingChoice).Append('|').Append(hand.PendingCard)
                .Append('|').Append(canUse).Append('|').Append(isLocalTurn).Append('|').Append(inJail).Append('|').Append((int)state);
            for (int i = 0; i < hand.Count; i++) signature.Append('|').Append((byte)CardInventoryRules.GetCard(hand, i));
            return signature.ToString();
        }

        void RenderCards(PlayerCardInventory hand, bool canUseJailCard, bool isLocalTurn, bool isInJail)
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
                bool canUse = canUseJailCard && cardType == PlayerCardType.GetOutOfJailFree;
                var card = new Button();
                card.AddToClassList("inventory-card");
                card.AddToClassList($"inventory-card-fan-{i}");
                card.clicked += () => HandleCardClick(card, (byte)slot, hand.HasPendingChoice, canUse);
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
                face.Add(StyledLabel(hand.HasPendingChoice ? "CLICK TO DISCARD" : canUse ? "CLICK TO PLAY" : CardCondition(cardType), "inventory-card-condition"));
                card.Add(face);
                cardsList.Add(card);
            }
        }

        void HandleCardClick(Button card, byte slot, bool pendingChoice, bool canUse)
        {
            if (card.ClassListContains("is-playing")) return;

            if (pendingChoice)
            {
                SendOverflowChoice(slot);
                return;
            }

            if (canUse)
            {
                card.AddToClassList("is-playing");
                root.schedule.Execute(() => SendUseRequest(slot)).ExecuteLater(240);
                return;
            }

            // Cards that cannot be played in the current state are informational only.
            // Leave the hand and its visual state unchanged when one is clicked.
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
                overlay.RemoveFromClassList("is-open");
                cardsPanel.style.display = DisplayStyle.None;
            }
        }

        void HandleCardsButtonMouseUp(MouseUpEvent evt)
        {
            if (evt.button != 0) return;
            SetInventoryOpen(true);
        }

        void SendUseRequest(byte slot)
        {
            var request = EntityManager.CreateEntity();
            EntityManager.AddComponentData(request, new UsePlayerCardRpc { Slot = slot });
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
            if (card != PlayerCardType.GetOutOfJailFree) return "EFFECT NOT READY";
            if (canUse) return "USE CARD";
            if (!isInJail) return "USE WHEN IN JAIL";
            return isLocalTurn ? "WAIT FOR YOUR TURN" : "USE ON YOUR TURN";
        }

        static string CardTitle(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "GET OUT OF JAIL FREE",
            _ => "MYSTERY CARD"
        };

        static string CardDescription(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "Keep this pass in your hand. Play it to leave Jail without spending a turn or rolling for doubles.",
            _ => "This card's effect is not available yet."
        };

        static string CardCondition(PlayerCardType card) => card switch
        {
            PlayerCardType.GetOutOfJailFree => "PLAYABLE WHILE YOU ARE IN JAIL",
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
