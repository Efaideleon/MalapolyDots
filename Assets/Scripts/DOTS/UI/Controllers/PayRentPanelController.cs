using System.Collections.Generic;
using DOTS.UI.Panels;
using DOTS.UI.Utilities.UIButtonEvents;

namespace DOTS.UI.Controllers
{
    public struct PayRentPanelContext
    {
        public int Rent;
    }

    public class PayRentPanelController
    {
        public PayRentPanel Panel { get; private set; }
        private readonly List<IButtonEvent> _buttonEvents;
        public PayRentPanelContext Context { get; set; }

        public PayRentPanelController(PayRentPanel panel, PayRentPanelContext context, List<IButtonEvent> buttonEvents)
        {
            Panel = panel;
            Context = context;
            _buttonEvents = buttonEvents;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            foreach (var buttonEvent in _buttonEvents)
                Panel.AcceptButton.clickable.clicked += buttonEvent.DispatchEvent;
            Panel.AcceptButton.clickable.clicked += Panel.Hide;
        }

        public void Update()
        {
            Panel.UpdateRentAmountLabel(Context.Rent.ToString());
        }

        public void Dispose()
        {
            foreach (var buttonEvent in _buttonEvents)
                Panel.AcceptButton.clickable.clicked += buttonEvent.DispatchEvent;
            Panel.AcceptButton.clickable.clicked -= Panel.Hide;
        }
    }
}
