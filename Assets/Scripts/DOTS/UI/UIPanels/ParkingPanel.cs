using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;

public class ParkingPanel : OnLandPanel
{
    public ParkingPanel(VisualElement parent) : base(parent.Q<VisualElement>("ParkingPanel"))
    {
        PanelType = SpaceTypeEnum.Parking;
        UpdateAcceptButtonReference("parking-panel-button");
        UpdateLabelReference("parking-panel-label");
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
            eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventsEnum.ChangeTurn });
            Hide();
        };
        AcceptButton.clickable.clicked += OnAcceptButton;
    }
}
