using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;

public class TreasurePanel : OnLandPanel
{
    public TreasurePanel(VisualElement parent) : base(parent.Q<VisualElement>("TreasurePanel"))
    {
        PanelType = SpaceTypeEnum.Treasure;
        UpdateAcceptButtonReference("treasure-panel-button");
        UpdateLabelReference("treasure-panel-label");
        Hide();
    }

    // Move to the parent class and only call a class that will be overridden
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
