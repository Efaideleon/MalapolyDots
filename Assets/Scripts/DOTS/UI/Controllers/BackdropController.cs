using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DOTS.UI.Controllers
{
    public class BackdropController
    {
        // Make the panel on appear, or be clickable only when one of the panels we are hiding are shown
        // TODO: Whenever one of the panels to hide appears, we should make the backdrop appear. 
        // Make subscribe to some event that a panel is visible and make the backdrop visible too
        public Button Backdrop { get; private set; } 
        // TODO: With the button just subscribe to it being clicked and then hid the button and the panel
        // TODO: We should use controllers to hide panels?
        public List<IPanel> PanelsToHideRegistry {get; private set; }
        private readonly List<IPanelController> _controllers; 

        public BackdropController(Button backdrop)
        {
            PanelsToHideRegistry = new();
            _controllers = new();
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

        public void HidePanelsAndButton()
        {
            HideRegisteredPanels();
            HideBackdrop();
        }

        public void RegisterController(IPanelController controller)
        {
            _controllers.Add(controller);
        }

        public void ShowBackdrop() 
        {
            Backdrop.style.display = DisplayStyle.Flex;
        }

        public void HideBackdrop() 
        {
            Backdrop.style.display = DisplayStyle.None;
        }

        public void HideRegisteredPanels()
        {
            // UnityEngine.Debug.Log("Hiding Registered panels");
            foreach (var panel in PanelsToHideRegistry)
                panel.Hide();

            foreach (var controller in _controllers)
                controller.HidePanel();
        }

        public void RegisterPanelToHide(IPanel panel)
        {
            PanelsToHideRegistry.Add(panel);
        }
    }
}
