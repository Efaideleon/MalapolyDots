using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct PurchasePropertyPanelContext
{
    public FixedString64Bytes Name; 
    public int Price; 
}

public class PurchasePropertyPanelController
{
    public PurchasePropertyPanel PurchasePropertyPanel { get; private set; }
    public PurchasePropertyPanelContext Context { get; set; } 
    public AudioClip ClickSound { get; private set; }
    public AudioSource AudioSource { get; private set; }
    private EntityQuery transactionEventQuery;

    public PurchasePropertyPanelController(
            PurchasePropertyPanel purchasePropertyPanel,
            PurchasePropertyPanelContext context,
            AudioClip clickSound,
            AudioSource audioSource)
    {
        ClickSound = clickSound;
        AudioSource = audioSource;
        PurchasePropertyPanel = purchasePropertyPanel;
        Context = context;
        SubscribeEvents();
    }

    public void Update()
    {
        PurchasePropertyPanel.NameLabel.text = Context.Name.ToString();
        PurchasePropertyPanel.PriceLabel.text = Context.Price.ToString();
    }

    public void ShowPanel() => PurchasePropertyPanel.Show();

    private void SubscribeEvents()
    {
        PurchasePropertyPanel.OkButton.clickable.clicked += DispatchEvents;
        PurchasePropertyPanel.OkButton.clickable.clicked += PlaySound;
    }

    private void PlaySound()
    {
        AudioSource.PlayOneShot(ClickSound);
    }

    private void DispatchEvents()
    {
        var eventQueue = transactionEventQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
        UnityEngine.Debug.Log("Dispatching buy property event");
        eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventType.Purchase });
        PurchasePropertyPanel.Hide();
    }

    public void SetTransactionEventQuery(EntityQuery query)
    {
        transactionEventQuery = query;
    }

    public void Dispose()
    {
        PurchasePropertyPanel.OkButton.clickable.clicked -= DispatchEvents;
        PurchasePropertyPanel.OkButton.clickable.clicked -= PlaySound;
    }
}
