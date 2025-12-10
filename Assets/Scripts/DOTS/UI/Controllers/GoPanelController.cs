using System;
using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct GoPanelContext
    {
        public string Title;
    }

    public class GoPanelController : IPanelControllerNew<GoPanelContext>
    {        
        public GoPanel Panel { get; private set; }
        public EntityQuery TransactionEventBusQuery { get; private set; }
        public GoPanelContext Context { get; set; }

        public GoPanelController(GoPanel panel, GoPanelContext context)
        {
            Panel = panel;
            Context = context;
            SubscribeEvents();
        }

        [Obsolete("This method is deprecated.")]
        public void ShowPanel() => Panel.Show();

        [Obsolete("This method is deprecated.")]
        public void HidePanel() => Panel.Hide();

        private void SubscribeEvents()
        {
            Panel.OkButton.clickable.clicked += DispatchEvents;
            Panel.OkButton.clickable.clicked += Panel.Hide;
        }

        [Obsolete("This method is deprecated.")]
        public void Update()
        {
            Panel.TitleLabel.text = Context.Title;
        }

        public void SetEventBufferQuery(EntityQuery query)
        {
            TransactionEventBusQuery = query;
        }

        private void DispatchEvents()
        {
            var eventBuffer = TransactionEventBusQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.Go });
            // TODO: Remove this, we don't want to change turns after paying taxes.
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.ChangeTurn });
        }

        public void Dispose()
        {
            Panel.OkButton.clickable.clicked -= DispatchEvents;
            Panel.OkButton.clickable.clicked -= Panel.Hide;
        }

        public void Show()
        {
            Panel.Show();
        }

        public void Hide()
        {
            Panel.Hide();
        }

        public void Update(GoPanelContext data)
        {
            Panel.TitleLabel.text = data.Title;
        }
    }
}
