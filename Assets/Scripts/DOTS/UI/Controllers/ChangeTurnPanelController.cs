using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct ChangeTurnPanelContext
    {
        public bool IsVisible;
    }

    public class ChangeTurnPanelController
    {
        public ChangeTurnPanel ChangeTurnPanel { get; private set; }
        public EntityQuery TransactionEventBufferQuery { get; private set; }
        public ChangeTurnPanelContext Context { get; set; }

        public ChangeTurnPanelController(ChangeTurnPanel panel, ChangeTurnPanelContext context)
        {
            ChangeTurnPanel = panel;
            Context = context;
            SubscribeEvents();
        }

        public void UpdateVisibility()
        {
            if (Context.IsVisible)
            {
                ChangeTurnPanel.Show();
            }
            else
            {
                ChangeTurnPanel.Hide();
            }
        }

        public void SubscribeEvents()
        {
            ChangeTurnPanel.ChangeTurnButton.clickable.clicked += DispatchEvents;
        }

        public void SetEventBufferQuery(EntityQuery query) => TransactionEventBufferQuery = query;

        private void DispatchEvents()
        {
            if (TransactionEventBufferQuery != null)
            {
                var eventBuffer = TransactionEventBufferQuery.GetSingletonBuffer<TransactionEventBuffer>();
                eventBuffer.Add(new TransactionEventBuffer{ EventType = TransactionEventType.ChangeTurn });
            }
        }

        public void Dispose()
        {
            ChangeTurnPanel.ChangeTurnButton.clickable.clicked -= DispatchEvents;
        }
    }
}
