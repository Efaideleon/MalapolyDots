using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;

public class JailPanel : OnLandPanel
{
    public JailPanel(VisualElement parent) : base(parent.Q<VisualElement>("JailPanel"))
    {
        PanelType = SpaceType.Jail;
        UpdateAcceptButtonReference("jail-panel-button");
        UpdateLabelReference("jail-panel-label");
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
            eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventType.ChangeTurn});
            Hide();
        };
        AcceptButton.clickable.clicked += OnAcceptButton;
    }
}
