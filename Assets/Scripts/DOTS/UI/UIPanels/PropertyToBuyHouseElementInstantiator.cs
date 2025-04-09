using UnityEngine;
using UnityEngine.UIElements;

public class PropertyToBuyHouseElementInstantiator
{
    public VisualElement ParentElement { get; private set; }
    private VisualTreeAsset propertyNameCounterUxml;

    public PropertyToBuyHouseElementInstantiator()
    {
        LoadPropertyNameCounterUXML();
    }

    private void LoadPropertyNameCounterUXML()
    {
        // TODO: Put the panel in the Resources folder
        propertyNameCounterUxml = Resources.Load<VisualTreeAsset>("BuyHouseNameAndCounterElement");
    }

    public VisualElement InstantiatePropertyNameCounterElement(VisualElement parent)
    {
        ParentElement = parent;
        VisualElement propertyNameCounterVE = null;
        if (propertyNameCounterUxml != null)
        {
            propertyNameCounterVE = propertyNameCounterUxml.Instantiate();
            // TODO: This panel should be a child of buy-house-ui-container in the BuyHousePanel
            var parentContainer = ParentElement.Q<VisualElement>("BuyHousePanel");
            if (parentContainer != null)
            {
                var propertyElementContainer = parentContainer.Q<VisualElement>("property-element");
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
