using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;

public class TaxPanel : OnLandPanel
{
    public TaxPanel(VisualElement parent) : base (parent.Q<VisualElement>("TaxPanel"))
    {
        PanelType = SpaceType.Tax;
        UpdateAcceptButtonReference("tax-panel-button");
        UpdateLabelReference("tax-panel-label");
        Hide();
    }

    public override void Show(ShowPanelContext context)
    {
        var name = context.entityManager.GetComponentData<NameComponent>(context.spaceEntity);
        UpdateTitleLabelText($"{name.Value}");
        Show();
    }

    public override void AddAcceptButtonAction(EntityQuery entityQuery)
    {
        OnAcceptButton = () => { 
            var eventQueue = entityQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
            eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventType.ChangeTurn });
            Hide();
        };
        AcceptButton.clickable.clicked += OnAcceptButton;
    }
}
