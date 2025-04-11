using System.Collections.Generic;
using Unity.Collections;
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
        public BuyHousePanel BuyHousePanel { get;private set; } 

        // TODO: Make sure to clear the list when the BuyHousePanel is closed.
        // TODO: And unsubcribe from the events attached to the Actions
        // TODO: PropertyNameCounterElement should be renamed since it not longer represents just a name and +/- 
        public List<PropertyNameCounterElement> PropertyNameCounterElementsList { get; private set; }

        public BuyHouseUI(VisualElement parent)
        {
            Root = parent.Q<VisualElement>("UpgradeHousePanel");
            buyHouseButton = Root.Q<Button>("buy-house-button");
            PropertyNameCounterElementsList = new();
            BuyHousePanel = new(Root.Q<VisualElement>("BuyHousePanel"));
        }
    }
}
