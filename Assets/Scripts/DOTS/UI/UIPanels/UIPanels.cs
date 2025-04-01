using UnityEngine.UIElements;
using Unity.Entities;
using System;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct ShowPanelContext
    {
        public Entity spaceEntity;
        public EntityManager entityManager;
        public int playerID;
    }
    
    public class OnLandPanel : Panel
    {
        public SpaceTypeEnum PanelType { get; protected set; }
        public OnLandPanel(VisualElement parent) : base(parent) { }
        public virtual void Show(ShowPanelContext context) { }
    }

    public class TopPanel
    {
        private readonly UIDocument uiDocument;
        public VisualElement Root { get; private set; }

        public TopPanel(UIDocument uiDocument)
        {
            this.uiDocument = uiDocument;
            Root = this.uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
        }
    }

    public class BotPanel
    {
        private readonly UIDocument uiDocument;
        public VisualElement Root { get; private set; }

        public BotPanel(UIDocument uiDocument)
        {
            this.uiDocument = uiDocument;
            Root = this.uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
        }
    }

    public class StatsPanel
    {
        public VisualElement Parent { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }

        public StatsPanel(VisualElement parent)
        {
            Parent = parent;
            PlayerNameLabel = Parent.Q<Label>("player-name-label");
            PlayerMoneyLabel = Parent.Q<Label>("player-money-label");
        }

        public void UpdatePlayerNameLabelText(string text)
        {
            PlayerNameLabel.text = text;
        }

        public void UpdatePlayerMoneyLabelText(string text)
        {
            PlayerMoneyLabel.text = text;
        }
    }

    public class RollPanel : Panel
    {
        public Button RollButton { get; private set; }
        public Label RollLabel { get; private set; }

        public Action OnRollButton;

        public RollPanel(VisualElement parent) : base(parent.Q<VisualElement>("RollPanel"))
        {
            RollButton = Root.Q<Button>("roll-button");
            RollLabel = Root.Q<Label>("roll-amount-label");
            Hide();
        }

        public void AddActionToRollButton(Action action)
        {
            OnRollButton = action;
            RollButton.clickable.clicked += OnRollButton;
        }

        private void UnsubscribeRollButton()
        {
            RollButton.clickable.clicked -= OnRollButton;
        }

        public void UpdateRollLabel(string text)
        {
            RollLabel.text = text;
        }

        public override void Show()
        {
            base.Show();
            UpdateRollLabel("0");
            ShowRollButton();
        }

        public void HideRollButton() => RollButton.style.display = DisplayStyle.None;
        public void ShowRollButton() => RollButton.style.display = DisplayStyle.Flex;

        public override void Dispose() => UnsubscribeRollButton();
    }
}
