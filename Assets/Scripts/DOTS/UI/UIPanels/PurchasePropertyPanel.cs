using UnityEngine.UIElements;
using Unity.Entities;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Collections;

public class PurchasePropertyPanel : Panel
{
    private Label PriceLabel;
    public FixedString64Bytes LandOnPropertyName { get; private set; }
    public int LandOnPropertyPrice { get; private set; }

    public PurchasePropertyPanel(VisualElement parent) : base(parent.Q<VisualElement>("PopupMenuPanel"))
    {
        UpdateAcceptButtonReference("popup-menu-accept-button");
        UpdateLabelReference("buy-popup-menu-label");
        SetPriceLabelReference("price-popup-menu-label");
        Hide();
    }

    private void SetPriceLabelReference(string className)
    {
        PriceLabel = Root.Q<Label>(className);
    }

    public void UpdatePriceLabelText(string text)
    {
        PriceLabel.text = text;
    }

    public void Show(ShowPanelContext context)
    {
        // TODO: This should be change to only reading context and not entityManager
        var name = context.entityManager.GetComponentData<NameComponent>(context.spaceEntity);
        var price = context.entityManager.GetComponentData<PriceComponent>(context.spaceEntity);
        LandOnPropertyName = name.Value;
        LandOnPropertyPrice = price.Value;
        UpdateTitleLabelText($"{LandOnPropertyName}");
        UpdatePriceLabelText($"{LandOnPropertyPrice}");
        Show();
    }

    // TODO: Remove the function because panels don't appear onLand anymore
    public override void AddAcceptButtonAction(EntityQuery entityQuery)
    {
        OnAcceptButton = () => { 
            var eventQueue = entityQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
            eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventType.Purchase });
            Hide();
        };
        AcceptButton.clickable.clicked += OnAcceptButton;
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
