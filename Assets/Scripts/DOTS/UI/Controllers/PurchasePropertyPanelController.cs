using System;
using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.UI.Controllers
{
    public struct PurchasePropertyPanelContext
    {
        public FixedString64Bytes Name;
        public int Price;
    }

    public class ManagedPurchasePropertyPanelContext
    {
        public Sprite sprite;
    }

    public class PurchasePropertyPanelController : IPanelControllerNew<PurchasePropertyPanelContext>
    {
        public PurchasePropertyPanel PurchasePropertyPanel { get; private set; }
        public PurchasePropertyPanelContext Context { get; set; }
        public ManagedPurchasePropertyPanelContext ManagedContext { get; set; }
        public AudioClip ClickSound { get; private set; }
        public AudioSource AudioSource { get; private set; }
        private EntityQuery transactionEventQuery;

        public PurchasePropertyPanelController(
                PurchasePropertyPanel purchasePropertyPanel,
                PurchasePropertyPanelContext context,
                ManagedPurchasePropertyPanelContext managedContext)
        {
            ManagedContext = managedContext;
            PurchasePropertyPanel = purchasePropertyPanel;
            Context = context;
            SubscribeEvents();
        }

        public void SetAudioSource(AudioSource audioSource) => AudioSource = audioSource;
        public void SetClickSound(AudioClip audioClip) => ClickSound = audioClip;

        [Obsolete("This method is deprecated")]
        public void Update()
        {
            PurchasePropertyPanel.Image.style.backgroundImage = new StyleBackground(ManagedContext.sprite);
            PurchasePropertyPanel.NameLabel.text = Context.Name.ToString();
            PurchasePropertyPanel.PriceLabel.text = Context.Price.ToString();
        }

        [Obsolete("This method is deprecated")]
        public void ShowPanel() => PurchasePropertyPanel.Show();

        private void SubscribeEvents()
        {
            // On Ok Pressed.
            PurchasePropertyPanel.OkButton.clickable.clicked += DispatchEvents;
            PurchasePropertyPanel.OkButton.clickable.clicked += PurchasePropertyPanel.Hide;
            PurchasePropertyPanel.OkButton.clickable.clicked += PlaySound;

            // On Decline Pressed.
            PurchasePropertyPanel.DeclineButton.clickable.clicked += PlaySound;
            PurchasePropertyPanel.DeclineButton.clickable.clicked += PurchasePropertyPanel.Hide;
        }

        private void PlaySound()
        {
            if (ClickSound != null)
            {
                AudioSource?.PlayOneShot(ClickSound);
            }
        }

        private void DispatchEvents()
        {
            var eventBuffer = transactionEventQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.Purchase });
        }

        public void SetEventBufferQuery(EntityQuery query)
        {
            transactionEventQuery = query;
        }

        public void Dispose()
        {
            // On Ok Pressed.
            PurchasePropertyPanel.OkButton.clickable.clicked -= DispatchEvents;
            PurchasePropertyPanel.OkButton.clickable.clicked -= PurchasePropertyPanel.Hide;
            PurchasePropertyPanel.OkButton.clickable.clicked -= PlaySound;

            // On Decline Pressed.
            PurchasePropertyPanel.DeclineButton.clickable.clicked += PlaySound;
            PurchasePropertyPanel.DeclineButton.clickable.clicked += PurchasePropertyPanel.Hide;
        }

        public void Update(PurchasePropertyPanelContext data)
        {
            // TODO: This line should use data.
            // TODO: A key should be passed on data. The sprite registry should be a depenency.
            PurchasePropertyPanel.Image.style.backgroundImage = new StyleBackground(ManagedContext.sprite); 

            PurchasePropertyPanel.NameLabel.text = data.Name.ToString();
            PurchasePropertyPanel.PriceLabel.text = data.Price.ToString();
        }

        public void Show()
        {
            PurchasePropertyPanel.Show();
        }

        public void Hide()
        {
            PurchasePropertyPanel.Hide();
        }
    }
}
