using UnityEngine;
using UnityEngine.UIElements;

public class PropertyPurchasePanelFactory
{
    public VisualElement ParentElement { get; private set; }
    private VisualTreeAsset propertyNameCounterUxml;

    public PropertyPurchasePanelFactory()
    {
        LoadPropertyNameCounterUXML();
    }

    private void LoadPropertyNameCounterUXML()
    {
        // TODO: Put the panel in the Resources folder
        propertyNameCounterUxml = Resources.Load<VisualTreeAsset>("BuyHouseNameAndCounterElement");
    }

    public VisualElement InstantiatePanel(VisualElement parent)
    {
        UnityEngine.Debug.Log("Instantiating propertyNameCounterUxml");
        ParentElement = parent;
        VisualElement propertyNameCounterVE = null;
        if (propertyNameCounterUxml != null)
        {
            propertyNameCounterVE = propertyNameCounterUxml.Instantiate();
            // TODO: This panel should be a child of buy-house-ui-container in the BuyHousePanel
            var parentContainer = ParentElement.Q<VisualElement>("BuyHousePanel");
            if (parentContainer != null)
            {
                var scrollView = parentContainer.Q<VisualElement>("properties_scrollview");
                var propertyElementContainer = scrollView.Q<VisualElement>("unity-content-container");
                propertyElementContainer.Add(propertyNameCounterVE);
            }
            else
            {
                Debug.Log("BuyHousePanel does not Exist");
            }
            // TODO: This is bogus instantiating something and adding it to something that is also instantiated
            // TODO: Fix the parent and Root difference
        }
        else
        {
            Debug.Log("housePanelUxml not loaded, check path to UpgradeHousePanel");
        }
        return propertyNameCounterVE;
    }
}
