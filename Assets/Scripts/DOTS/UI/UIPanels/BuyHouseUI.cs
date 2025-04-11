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

    // TODO: Currently this class is serving a both a UI and a form a controller
    // Will be better to split up so that it makes more sense, and it is easier to understand
    // what this class does.
    public class BuyHouseUI
    {
        public VisualElement Root;
        public Button buyHouseButton;
        private readonly PropertyToBuyHouseElementInstantiator propertyNameCounterInstantiator;
        public List<string> PropertiesToBuyHouses { get; private set; }
        private BuyHousePanel buyHousePanel; 

        // TODO: Make sure to clear the list when the BuyHousePanel is closed.
        // TODO: And unsubcribe from the events attached to the Actions
        // TODO: PropertyNameCounterElement should be renamed since it not longer represents just a name and +/- 
        public List<PropertyNameCounterElement> PropertyNameCounterElementsList { get; private set; }

        private EntityQuery buyHouseEventsQuery;

        public BuyHouseUI(VisualElement parent)
        {
            PropertiesToBuyHouses = new();
            Root = parent.Q<VisualElement>("UpgradeHousePanel");
            buyHouseButton = Root.Q<Button>("buy-house-button");
            propertyNameCounterInstantiator = new();
            PropertyNameCounterElementsList = new();
            buyHousePanel = new(Root.Q<VisualElement>("BuyHousePanel"));
            SubscribeEvents();
        }

        // once all the house have been bought for a property remove that panel or gray it out.
        // so that no more house be bought for it.
        public void AddPropertyName(string name)
        {
            UnityEngine.Debug.Log($"Add Property Name {name}");
            PropertiesToBuyHouses.Add(name);
        }

        public void SetBuyHouseEventQuery(EntityQuery entityQuery)
        {
            buyHouseEventsQuery = entityQuery;
        }

        public void SubscribeEvents()
        {
            buyHouseButton.clickable.clicked += ShowBuyHousePanel;
        }

        public void Dispose()
        {
            buyHouseButton.clickable.clicked -= ShowBuyHousePanel;
        }

        private void SendBuyHouseEvents(ToggleState toggleState, PropertyNameCounterElementContext context)
        {
            switch(toggleState)
            {
                case ToggleState.Buy:
                    var eventBuffer = buyHouseEventsQuery.GetSingletonBuffer<BuyHouseEvent>();
                    foreach (var buyHouseEvent in GetListOfBuyHouseEvents(context))
                    {
                        UnityEngine.Debug.Log($"Buy a house for {buyHouseEvent.property}");
                        eventBuffer.Add(buyHouseEvent);
                    }
                    break;
                case ToggleState.Sell:
                    UnityEngine.Debug.Log("Selling abel houses lol");
                    break;
            }
        }

        private void ShowBuyHousePanel()
        {
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
                // TODO: Call dispose when the BuyHousePanel is closed.
                var propertyPanel = new PropertyNameCounterElement(propertyPanelVE, propertyNameCounterContext);
                propertyPanel.OnOkClicked += SendBuyHouseEvents;
                // Keeping track of how many buy/sell property elements we instantiated
                PropertyNameCounterElementsList.Add(propertyPanel);
            }
            buyHousePanel.Show();

            PropertiesToBuyHouses.Clear();
        }

        // Creates a list of events for each selected house to buy
        private List<BuyHouseEvent> GetListOfBuyHouseEvents(PropertyNameCounterElementContext context)
        {
            List<BuyHouseEvent> listOfBuyHouseEvents = new();
            UnityEngine.Debug.Log($"Buying hosues for {context.Name}");
            foreach (var panel in PropertyNameCounterElementsList)
            {
                if (context.Name == panel.Context.Name)
                {
                    for (int i = 0; i < panel.NumOfHousesToBuy; i++)
                    {
                        listOfBuyHouseEvents.Add(new BuyHouseEvent { property = panel.Context.Name });
                    }
                }
            }
            return listOfBuyHouseEvents;
        }
    }
}
