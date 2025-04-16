using UnityEngine.UIElements;
using Unity.Entities;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public class YouBoughtPanel : Panel
    {
        public YouBoughtPanel(VisualElement parent) : base(parent.Q<VisualElement>("YouBoughtPanel"))
        {
            UpdateAcceptButtonReference("you-bought-ok-button");
            UpdateLabelReference("you-bought-label");
            Hide();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            OnAcceptButton = () =>
            {
                var eventQueue = entityQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.ChangeTurn });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }
        
        public void Show(ShowPanelContext context)
        {
            var name = context.entityManager.GetComponentData<NameComponent>(context.spaceEntity).Value;
            UpdateTitleLabelText($"You bought: {name}");
            Show();
        }
    }

    public class PayRentPanel : Panel
    {
        public PayRentPanel(VisualElement parent) : base(parent.Q<VisualElement>("PayRentPanel"))
        { 
            UpdateAcceptButtonReference("pay-rent-ok-button");
            UpdateLabelReference("pay-rent-label");
            Hide();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            // Show pay the rent first and then change turn
            OnAcceptButton = () =>
            {
                var eventQueue = entityQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.PayRent });
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.ChangeTurn });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public void Show(ShowPanelContext context)
        {
            var rent = context.entityManager.GetComponentData<RentComponent>(context.spaceEntity).Value;
            UpdateTitleLabelText($"Pay Rent: {rent}");
            Show();
        }
    }

    // This behaves more like a controller, determining what panels to show and changing their content.
    // TODO: DELETE This code
    public class PropertyPanel : OnLandPanel
    {
        private readonly YouBoughtPanel youBoughtPanel; 
        private readonly PayRentPanel payRentPanel;
        private readonly PurchasePropertyPanel buyPanel;
        private ShowPanelContext showPanelContext;

        public PropertyPanel(VisualElement parent) : base(parent)
        {
            PanelType = SpaceType.Property;
            youBoughtPanel = new(parent);
            payRentPanel = new(parent);
            buyPanel = new(parent);
            buyPanel.AcceptButton.clicked += OnBuyPanelAcceptClicked;
        }

        public override void Show(ShowPanelContext context)
        {
            showPanelContext = context;
            var ownerID = context.entityManager.GetComponentData<OwnerComponent>(context.spaceEntity).ID;
            if (IsSpaceFree(ownerID)) 
            {
                buyPanel.Show(context);
            }
            if (!IsSpaceFree(ownerID) && !IsPlayerOwner(ownerID, context.playerID)) 
            {
                payRentPanel.Show(context);
           }
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            buyPanel.AddAcceptButtonAction(entityQuery);
            payRentPanel.AddAcceptButtonAction(entityQuery);
            youBoughtPanel.AddAcceptButtonAction(entityQuery);
        }

        private bool IsPlayerOwner(int ownerID, int playerID)
        {
            return ownerID == playerID;
        }

        private bool IsSpaceFree(int ownerID)
        {
            return ownerID == PropertyConstants.Vacant;
        }

        private void OnBuyPanelAcceptClicked()
        {
            // youBoughtPanel.Show(showPanelContext);
        }

        public override void Dispose()
        {
            base.Dispose();
            buyPanel.AcceptButton.clicked -= OnBuyPanelAcceptClicked;
            buyPanel.Dispose();
            payRentPanel.Dispose();
            youBoughtPanel.Dispose();
        }
    }
}
