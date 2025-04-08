using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct BuyHousePanelContext
    {
        public FixedString64Bytes Name { get; set; }
        public int Price { get; set; }
    }
    public class BuyHousePanel : Panel
    {
        public Label PriceLabel { get; private set; }
        public Button DeclineButton { get; private set; }
        public BuyHousePanelContext context;

        public BuyHousePanel(VisualElement parent, BuyHousePanelContext context) : base(parent.Q<VisualElement>("upgrade-house-panel"))
        {
            UpdateAcceptButtonReference("upgrade-house-accept-button");
            UpdateLabelReference("upgrade-house-title-label");
            PriceLabel = Root.Q<Label>("upgrade-house-price-label");
            DeclineButton = Root.Q<Button>("upgrade-house-decline-button");
            this.context = context;
            Hide();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            OnAcceptButton = () =>
            {
                var eventBuffer = entityQuery.GetSingletonBuffer<BuyHouseEvent>();
                eventBuffer.Add(new BuyHouseEvent { property = context.Name });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public override void Show()
        {
            // get the house level here
            UpdateTitleLabelText($"{context.Name} House level: {1}");
            PriceLabel.text = $"{1} Abel";
            Show();
        }
    }

    public class BuyHouseUI
    {
        public VisualElement Root;
        public Button buyHouseButton;
        public BuyHousePanel buyHousePanel;
        public List<string> PropertiesToBuyHouses { get; private set; }
        private EntityQuery buyHouseEventsQuery;

        public BuyHouseUI(VisualElement parent)
        {
            PropertiesToBuyHouses = new();
            Root = parent.Q<VisualElement>("UpgradeHousePanel");
            buyHouseButton = Root.Q<Button>("buy-house-button");
            SubscribeEvents();
        }

        // once all the house have been bought for a property remove that panel or gray it out.
        // so that no more house be bought for it.
        public void AddPropertyName(string name) => PropertiesToBuyHouses.Add(name);

        public void AddBuyHouseEventQuery(EntityQuery entityQuery)
        {
            buyHouseEventsQuery = entityQuery;
        }

        public void SubscribeEvents()
        {
            buyHouseButton.clickable.clicked += ShowBuyHousePanels;
        }

        public void Dispose()
        {
            buyHouseButton.clickable.clicked -= ShowBuyHousePanels;
        }

        private void ShowBuyHousePanels()
        {
            // Check if the current player has any monopolies over a color
            // if they do then show him the panel to buy a house there
            foreach (var name in PropertiesToBuyHouses)
            {
                var panelContext = new BuyHousePanelContext
                {
                    Name = name,
                    Price = 0
                };

                buyHousePanel = new BuyHousePanel(Root, panelContext);
                buyHousePanel.AddAcceptButtonAction(buyHouseEventsQuery);
                buyHousePanel.Show();
            }
            PropertiesToBuyHouses.Clear();
        }
    }
}
