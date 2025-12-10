using System;
using System.Collections.Generic;
using DOTS.UI.Panels;
using DOTS.UI.Utilities.UIButtonEvents;

namespace DOTS.UI.Controllers
{
    public struct PayTaxPanelContext
    {
        public int Amount;
    }

    public class PayTaxPanelController : IPanelControllerNew<PayTaxPanelController>
    {
        public TaxPanel Panel { get; private set; }
        private readonly List<IButtonEvent> _buttonEvents;
        public PayTaxPanelContext Context { get; set; }

        public PayTaxPanelController(TaxPanel panel, PayTaxPanelContext context, List<IButtonEvent> buttonEvents)
        {
            Panel = panel;
            Context = context;
            _buttonEvents = buttonEvents;
            SubscribeEvents();
        }

        [Obsolete("This method is deprecated.")]
        public void ShowPanel() => Panel.Show();

        [Obsolete("This method is deprecated.")]
        public void HidePanel() => Panel.Hide();

        private void SubscribeEvents()
        {
            foreach(var buttonEvent in _buttonEvents)
                Panel.OkButton.clickable.clicked += buttonEvent.DispatchEvent;
            Panel.OkButton.clickable.clicked += Panel.Hide;
        }

        [Obsolete("This method is deprecated.")]
        public void Update()
        {
            Panel.AmountLabel.text = Context.Amount.ToString();
        }

        public void Dispose()
        {
            foreach(var buttonEvent in _buttonEvents)
                Panel.OkButton.clickable.clicked += buttonEvent.DispatchEvent;
            Panel.OkButton.clickable.clicked -= Panel.Hide;
        }

        public void Update(PayTaxPanelController data)
        {
            Panel.AmountLabel.text = Context.Amount.ToString();
        }

        public void Show()
        {
            Panel.Show();
        }

        public void Hide()
        {
            Panel.Hide();
        }
    }
}
