using System;
using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct ChancePanelContext
    {
        public string Title;
    }
    public class ChancePanelController : IPanelControllerNew<ChancePanelContext>
    {
        public ChancePanel Panel { get; private set; }
        public EntityQuery TransactionEventBusQuery { get; private set; }
        public ChancePanelContext Context { get; set; }

        public ChancePanelController(ChancePanel panel, ChancePanelContext context)
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
        public void Update(ref ChancePanelContext context)
        {
            Panel.TitleLabel.text = context.Title;
        }

        public void SetEventBufferQuery(EntityQuery query)
        {
            TransactionEventBusQuery = query;
        }

        private void DispatchEvents()
        {
            var eventBuffer = TransactionEventBusQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.Chance });
        }

        public void Dispose()
        {
            Panel.OkButton.clickable.clicked -= DispatchEvents;
            Panel.OkButton.clickable.clicked -= Panel.Hide;
        }

        public void Update(ChancePanelContext data)
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
