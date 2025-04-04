using Unity.Entities;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public class BuyHousePanel : Panel
    {
        public Label PriceLabel { get; private set; }
        public Button DeclineButton { get; private set; }

        public BuyHousePanel(VisualElement parent) : base(parent.Q<VisualElement>("upgrade-house-panel"))
        {
            UpdateAcceptButtonReference("upgrade-house-accept-button");
            UpdateLabelReference("upgrade-house-title-label");
            PriceLabel = Root.Q<Label>("upgrade-house-price-label");
            DeclineButton = Root.Q<Button>("upgrade-house-decline-button");
            Hide();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            OnAcceptButton = () =>
            {
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.UpgradeHouse });
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.ChangeTurn });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public void Show(ShowPanelContext context)
        {
            // get the house level here
            UpdateTitleLabelText($"House level: {1}");
            PriceLabel.text = $"{1} Abel";
            Show();
        }
    }

    public class BuyHouseUI
    {
        public VisualElement Root;
        public Button buyHouseButton;
        public BuyHousePanel buyHousePanel;

        public BuyHouseUI(VisualElement parent)
        {
            Root = parent.Q<VisualElement>("UpgradeHousePanel");
            buyHouseButton = Root.Q<Button>("buy-house-button");
            buyHousePanel = new BuyHousePanel(Root);
            SubscribeEvents();
        }

        public void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            buyHousePanel.AddAcceptButtonAction(entityQuery);
        }

        public void SubscribeEvents()
        {
            buyHouseButton.clickable.clicked += ShowBuyHousePanel;
        }

        public void Dispose()
        {
            buyHouseButton.clickable.clicked -= ShowBuyHousePanel;
        }

        public void ShowBuyHousePanel()
        {
            buyHousePanel.Show();
        }
    }
}
