using System;
using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct TreasurePanelContext
    {
        public string Title;
    }

    public class TreasurePanelController : IPanelControllerNew<TreasurePanelContext>
    {
        public TreasurePanel Panel { get; private set; }
        public EntityQuery TransactionEventBusQuery { get; private set; }
        public TreasurePanelContext Context { get; set; }

        public TreasurePanelController(TreasurePanel panel, TreasurePanelContext context)
        {
            Panel = panel;
            Context = context;
            SubscribeEvents();
        }

        [Obsolete("This method is deprecated")]
        public void ShowPanel() => Panel.Show();

        [Obsolete("This method is deprecated")]
        public void HidePanel() => Panel.Hide();

        private void SubscribeEvents()
        {
            Panel.OkButton.clickable.clicked += DispatchEvents;
            Panel.OkButton.clickable.clicked += Panel.Hide;
        }

        [Obsolete("This method is deprecated")]
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
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.Treasure });
        }

        public void Dispose()
        {
            Panel.OkButton.clickable.clicked -= DispatchEvents;
            Panel.OkButton.clickable.clicked -= Panel.Hide;
        }

        public void Update(TreasurePanelContext data)
        {
            Panel.TitleLabel.text = data.Title;
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
