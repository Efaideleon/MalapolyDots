using System.Collections.Generic;
using UnityEngine.UIElements;

// TODO: Make this a into a giant button
public class BackdropController
{
    // Make the panel on appear, or be clickable only when one of the panels we are hiding are shown
    // TODO: Whenever one of the panels to hide appears, we should make the backdrop appear. 
    // Make subscribe to some event that a panel is visible and make the backdrop visible too
    public Button Backdrop { get; private set; } 
    // TODO: With the button just subscribe to it being clicked and then hid the button and the panel
    public List<VisualElement> PanelsToHideRegistry {get; private set; }

    public BackdropController(Button backdrop)
    {
        PanelsToHideRegistry = new();
        if (backdrop == null)
        {
            UnityEngine.Debug.LogWarning("backdrop is null");
        }
        else
        {
            Backdrop = backdrop;
            UnityEngine.Debug.Log("Loading Backdrop");
            SubscribeEvents();
        }
        HideBackdrop();
    }

    private void SubscribeEvents()
    {
        Backdrop.clickable.clicked += HidePanelsAndButton;
    }

    public void Dispose()
    {
        Backdrop.clickable.clicked -= HidePanelsAndButton;
    }

    private void HidePanelsAndButton()
    {
        HideRegisteredPanels();
        HideBackdrop();
    }

    public void ShowBackdrop() 
    {
        UnityEngine.Debug.Log("Showing backdrop");
        Backdrop.style.display = DisplayStyle.Flex;
    }

    public void HideBackdrop() 
    {
        Backdrop.style.display = DisplayStyle.None;
        UnityEngine.Debug.Log("Hiding backdrop");
    }

    public void HideRegisteredPanels()
    {
        foreach (var panel in PanelsToHideRegistry)
        {
            // TODO: Call on the Hide function from the panel here
            // TODO: Use an interface?
            UnityEngine.Debug.Log("Hiding panels");
            panel.style.display = DisplayStyle.None;
        }
    }

    public void RegisterPanelToHide(VisualElement panel)
    {
        PanelsToHideRegistry.Add(panel);
    }
}
