using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

public class BuyHouseUIController
{
    public List<string> PropertiesToBuyHouses { get; private set; }
    public BuyHouseUI BuyHouseUI { get; private set; }
    private EntityQuery buyHouseEventsQuery;
    private readonly PropertyToBuyHouseElementInstantiator propertyNameCounterInstantiator;

    public BuyHouseUIController(BuyHouseUI buyHouseUI)
    {
        BuyHouseUI = buyHouseUI;
        PropertiesToBuyHouses = new();
        propertyNameCounterInstantiator = new();
        SubscribeEvents();
    }

    // once all the house have been bought for a property remove that panel or gray it out.
    // so that no more house be bought for it.
    public void AddPropertyName(string name)
    {
        UnityEngine.Debug.Log($"Add Property Name {name}");
        PropertiesToBuyHouses.Add(name);
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
            var propertyPanelVE = propertyNameCounterInstantiator.InstantiatePropertyNameCounterElement(BuyHouseUI.Root);
            // TODO: Call dispose when the BuyHousePanel is closed.
            var propertyPanel = new PropertyNameCounterElement(propertyPanelVE, propertyNameCounterContext);
            propertyPanel.OnOkClicked += SendBuyHouseEvents;
            // Keeping track of how many buy/sell property elements we instantiated
            BuyHouseUI.PropertyNameCounterElementsList.Add(propertyPanel);
        }
        BuyHouseUI.BuyHousePanel.Show();

        PropertiesToBuyHouses.Clear();
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

    public void SetBuyHouseEventQuery(EntityQuery entityQuery)
    {
        buyHouseEventsQuery = entityQuery;
    }

    private List<BuyHouseEvent> GetListOfBuyHouseEvents(PropertyNameCounterElementContext context)
    {
        List<BuyHouseEvent> listOfBuyHouseEvents = new();
        UnityEngine.Debug.Log($"Buying hosues for {context.Name}");
        foreach (var panel in BuyHouseUI.PropertyNameCounterElementsList)
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

    private void SubscribeEvents()
    {
        BuyHouseUI.buyHouseButton.clickable.clicked += ShowBuyHousePanel;
    }

    public void Dispose()
    {
        BuyHouseUI.buyHouseButton.clickable.clicked -= ShowBuyHousePanel;
    }
}
