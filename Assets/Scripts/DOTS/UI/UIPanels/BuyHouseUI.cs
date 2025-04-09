using System;
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

    public struct PropertyNameCounterElementContext
    {
        public FixedString64Bytes Name { get; set; }
        public int Price { get; set; }
    }

    public class PropertyNameCounterElement
    {
        public VisualElement Root { get; private set; }
        public Label PropertyName { get; private set; }
        public Label HouseCounter { get; private set; }
        public Button PlusButton { get; private set; }
        public Button MinusButton { get; private set; }
        public int NumOfHousesToBuy { get; private set; }
        public PropertyNameCounterElementContext Context {get; private set; }

        public PropertyNameCounterElement(VisualElement root, PropertyNameCounterElementContext context)
        {
            Context = context;
            Root = root;
            PropertyName = Root.Q<Label>("property-name"); 
            HouseCounter = Root.Q<Label>("houses-counter"); 
            MinusButton = Root.Q<Button>("subtract-houses-amount");
            PlusButton = Root.Q<Button>("add-houses-amount");

            PropertyName.text = Context.Name.ToString();
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            if (PlusButton != null)
            {
                PlusButton.clickable.clicked += IncreaseNumOfHouseToBuy;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Plus button is null");
            }
            if (MinusButton != null)
            {
                MinusButton.clickable.clicked += DecreaseNumOfHouseToBuy;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Minus button is null");
            }
        }

        // TODO: Must call when the element/BuyHousePanel gets destroyed
        public void Dispose()
        {
            PlusButton.clickable.clicked -= IncreaseNumOfHouseToBuy;
            MinusButton.clickable.clicked -= DecreaseNumOfHouseToBuy;
        }

        // Later we may want to send the events to a system to check if a house can be bought at all
        // To prevent increase the number of the UI events it no houses can be bought
        private void IncreaseNumOfHouseToBuy() 
        {
            UnityEngine.Debug.Log("Increasing num of house to buy");
            NumOfHousesToBuy++;
            UpdateNumOfHouseToBuyLabel();
        }

        private void DecreaseNumOfHouseToBuy() 
        {
            UnityEngine.Debug.Log("Decreasing num of house to buy");
            NumOfHousesToBuy--;
            UpdateNumOfHouseToBuyLabel();
        }

        private void UpdateNumOfHouseToBuyLabel()
        {
            if (HouseCounter != null)
            {
                HouseCounter.text = NumOfHousesToBuy.ToString();
            }
            else
            {
                UnityEngine.Debug.LogWarning("HouseCounter Label is null");
            }
        }
    }

    // TODO: This shouldn't inherit from Panel
    // It's different from regualr popup panels
    public class BuyHousePanel : Panel
    {
        public Label PriceLabel { get; private set; }
        public Button DeclineButton { get; private set; }
        public List<BuyHouseEvent> ListOfBuyHouseEvents { get; private set; } 

        // TODO: Change the class names
        // The parent for the panel is the BuyHousePanel, which is more like the Root than the panel
        public BuyHousePanel(VisualElement parent) : base(parent.Q<VisualElement>("upgrade-house-container"))
        {
            UpdateAcceptButtonReference("upgrade-house-accept-button");
            UpdateLabelReference("upgrade-house-title-label");
            PriceLabel = Root.Q<Label>("upgrade-house-price-label");
            DeclineButton = Root.Q<Button>("upgrade-house-decline-button");
            Hide();
        }

        public void AddBuyHouseEvent(BuyHouseEvent e)
        {
            ListOfBuyHouseEvents.Add(e);
        }

        public void AddAcceptButtonAction(EntityQuery entityQuery, Func<List<BuyHouseEvent>> GetListOfBuyHouseEvents)
        {
            OnAcceptButton = () =>
            {
                var eventBuffer = entityQuery.GetSingletonBuffer<BuyHouseEvent>();
                // The BuyHouseEvent represents purchasing one house
                foreach (var e in GetListOfBuyHouseEvents())
                {
                    UnityEngine.Debug.Log($"Buy a hosue for {e.property}");
                    eventBuffer.Add(e);
                }
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        // Call base or it causes Recursion
        // TODO: FIX this is redundant
        public override void Show()
        {
            base.Show();
        }
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
            buyHouseButton.clickable.clicked += ShowBuyHousePanels;
        }

        public void Dispose()
        {
            buyHouseButton.clickable.clicked -= ShowBuyHousePanels;
        }

        private void ShowBuyHousePanels()
        {
            // Check if the current player has any monopolies over a color
            // if they do then create the panel to buy a house
            // TODO: Make this into a hide and show panel instead of instantiating
            buyHousePanel.Show();

            foreach (var name in PropertiesToBuyHouses)
            {
                var propertyNameCounterContext = new PropertyNameCounterElementContext
                {
                    Name = name,
                    Price = 0,
                };

                // instantiating the uxml
                var propertyPanelVE = propertyNameCounterInstantiator.InstantiatePropertyNameCounterElement(Root);
                // passing the reference to a setup/controller class
                var propertyPanel = new PropertyNameCounterElement(propertyPanelVE, propertyNameCounterContext);
                // keeping track of how many elements we instantiated
                PropertyNameCounterElementsList.Add(propertyPanel);
            }

            // buyHouseVE is the BuyHousePanel
            // GetListOfBuyHouseEvents function crashes UnityEditor
            // When Accept is clicked we should pass the list of BuyHouseEvents
            buyHousePanel.AddAcceptButtonAction(buyHouseEventsQuery, GetListOfBuyHouseEvents);
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
