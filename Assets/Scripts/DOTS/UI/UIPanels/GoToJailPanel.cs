using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;

public class GoToJailPanel : OnLandPanel
{
    public GoToJailPanel(VisualElement parent) : base(parent.Q<VisualElement>("GoToJailPanel"))
    {
        PanelType = SpaceTypeEnum.GoToJail;
        UpdateAcceptButtonReference("go-to-jail-panel-button");
        UpdateLabelReference("go-to-jail-panel-label");
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
            var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
            eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventsEnum.ChangeTurn });
            Hide();
        };
        AcceptButton.clickable.clicked += OnAcceptButton;
    }
}
