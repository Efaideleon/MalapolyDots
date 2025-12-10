using System;
using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct JailPanelContext
    {
        public string Title;
    }

    public class JailPanelController : IDisposable, IPanelControllerNew<JailPanelContext>
    {        
        public JailPanel Panel { get; private set; }
        public EntityQuery TransactionEventBusQuery { get; private set; }
        public JailPanelContext Context { get; set; }

        public JailPanelController(JailPanel panel, JailPanelContext context)
        {
            Panel = panel ?? throw new ArgumentNullException($"[JailPanelController] {nameof(panel)} is null.");
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
            if (TransactionEventBusQuery == null)
            {
                UnityEngine.Debug.LogError($"[JailPanelController] {nameof(TransactionEventBusQuery)} has not been assigned.");
                return;
            }
            var eventBuffer = TransactionEventBusQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.Jail });
            // TODO: Remove this, we don't want to change turns after paying taxes.
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.ChangeTurn });
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (isDisposed)
                return;

            Panel.OkButton.clickable.clicked -= DispatchEvents;
            Panel.OkButton.clickable.clicked -= Panel.Hide;
            isDisposed = true;
        }

        public void Update(JailPanelContext data)
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
