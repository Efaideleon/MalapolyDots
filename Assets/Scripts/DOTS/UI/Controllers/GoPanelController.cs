using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct GoPanelContext
    {
        public string Title;
    }

    public class GoPanelController
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

        public void ShowPanel() => Panel.Show();
        public void HidePanel() => Panel.Hide();

        private void SubscribeEvents()
        {
            Panel.OkButton.clickable.clicked += DispatchEvents;
            Panel.OkButton.clickable.clicked += Panel.Hide;
        }

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
    }
}
