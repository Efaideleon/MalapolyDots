using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class YouBoughtPanel
    {
        public VisualElement Panel { get; private set; }
        public Button OkButton { get; private set; }
        public Label Title { get; private set; }

        public YouBoughtPanel(VisualElement parent)
        {
            Panel = parent.Q<VisualElement>("YouBoughtPanel");
            OkButton = Panel.Q<Button>("you-bought-ok-button");
            Title = Panel.Q<Label>("you-bought-label");
            Hide();
        }

        public void Hide() => Panel.style.display = DisplayStyle.None;
        public void Show() => Panel.style.display = DisplayStyle.Flex;

        // public override void AddAcceptButtonAction(EntityQuery entityQuery)
        // {
        //     OnAcceptButton = () =>
        //     {
        //         var eventQueue = entityQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
        //         eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.ChangeTurn });
        //         Hide();
        //     };
        //     AcceptButton.clickable.clicked += OnAcceptButton;
        // }
    }

}
