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

    public class BuyHouseUI
    {
        public VisualElement Root;
        public Button buyHouseButton;
        private readonly PropertyToBuyHouseElementInstantiator propertyNameCounterInstantiator;
        public BuyHousePanel buyHousePanel;
        public List<string> PropertiesToBuyHouses { get; private set; }

        // TODO: Make sure to clear the list when the BuyHousePanel is closed.
        public List<PropertyNameCounterElement> PropertyNameCounterElementsList { get; private set; }

        private EntityQuery buyHouseEventsQuery;

        public BuyHouseUI(VisualElement parent)
        {
            PropertiesToBuyHouses = new();
            Root = parent.Q<VisualElement>("UpgradeHousePanel");
            buyHouseButton = Root.Q<Button>("buy-house-button");
            buyHousePanel = new BuyHousePanel(Root.Q<VisualElement>("BuyHousePanel"));
            propertyNameCounterInstantiator = new();
            PropertyNameCounterElementsList = new();
            SubscribeEvents();
        }

        // once all the house have been bought for a property remove that panel or gray it out.
        // so that no more house be bought for it.
        public void AddPropertyName(string name)
        {
            UnityEngine.Debug.Log($"Add Property Name {name}");
            PropertiesToBuyHouses.Add(name);
        }

        public void AddBuyHouseEventQuery(EntityQuery entityQuery)
        {
            buyHouseEventsQuery = entityQuery;
        }

        public void SubscribeEvents()
        {
            buyHouseButton.clickable.clicked += ShowBuyHousePanel;
            buyHousePanel.BuyButton.clickable.clicked += SendBuyHouseEvents;
        }

        public void Dispose()
        {
            buyHouseButton.clickable.clicked -= ShowBuyHousePanel;
            buyHousePanel.BuyButton.clickable.clicked -= SendBuyHouseEvents;
        }

        private void SendBuyHouseEvents()
        {
            var eventBuffer = buyHouseEventsQuery.GetSingletonBuffer<BuyHouseEvent>();
            foreach (var buyHouseEvent in GetListOfBuyHouseEvents())
            {
                UnityEngine.Debug.Log($"Buy a house for {buyHouseEvent.property}");
                eventBuffer.Add(buyHouseEvent);
            }
            buyHousePanel.Hide();
        }

        private void ShowBuyHousePanel()
        {
            buyHousePanel.Show();

            // Check if the current player has any monopolies over a color
            // if they do then create the panel to buy a house
            foreach (var name in PropertiesToBuyHouses)
            {
                var propertyNameCounterContext = new PropertyNameCounterElementContext
                {
                    Name = name,
                    Price = 0,
                };

                // Instantiating the uxml
                var propertyPanelVE = propertyNameCounterInstantiator.InstantiatePropertyNameCounterElement(Root);
                var propertyPanel = new PropertyNameCounterElement(propertyPanelVE, propertyNameCounterContext);
                // Keeping track of how many elements we instantiated
                PropertyNameCounterElementsList.Add(propertyPanel);
            }

            buyHousePanel.Show();
            PropertiesToBuyHouses.Clear();
        }

        // Creates a list of events for each selected house to buy
        private List<BuyHouseEvent> GetListOfBuyHouseEvents()
        {
            List<BuyHouseEvent> listOfBuyHouseEvents = new();
            foreach (var panel in PropertyNameCounterElementsList)
            {
                for (int i = 0; i < panel.NumOfHousesToBuy; i++)
                {
                    listOfBuyHouseEvents.Add(new BuyHouseEvent { property = panel.Context.Name });
                }
            }
            return listOfBuyHouseEvents;
        }
    }
}
