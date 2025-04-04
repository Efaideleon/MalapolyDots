using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Collections;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public class UpgradeHousePanel : Panel
    {
        public Label PriceLabel { get; private set; }
        public Button DeclineButton { get; private set; }

        public UpgradeHousePanel(VisualElement parent) : base(parent.Q<VisualElement>("UpgradeHousePanel"))
        {
            UpdateAcceptButtonReference("upgrade-house-accept-button");
            UpdateLabelReference("upgrade-house-title-label");
            PriceLabel = Root.Q<Label>("upgrade-house-price-label");
            DeclineButton = Root.Q<Button>("upgrade-house-decline-button");
            Hide();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            OnAcceptButton = () =>
            {
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.UpgradeHouse });
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.ChangeTurn });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public void Show(ShowPanelContext context)
        {
            // get the house level here
            UpdateTitleLabelText($"House level: {1}");
            PriceLabel.text = $"{1} Abel";
            Show();
        }
    }

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
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.ChangeTurn });
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
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.PayRent });
                eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventsEnum.ChangeTurn });
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

    public class BuyPanel : Panel
    {
        private Label PriceLabel;
        public FixedString64Bytes LandOnPropertyName { get; private set; }
        public int LandOnPropertyPrice { get; private set; }

        public BuyPanel(VisualElement parent) : base(parent.Q<VisualElement>("PopupMenuPanel"))
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
            var name = context.entityManager.GetComponentData<NameComponent>(context.spaceEntity);
            var price = context.entityManager.GetComponentData<PriceComponent>(context.spaceEntity);
            LandOnPropertyName = name.Value;
            LandOnPropertyPrice = price.Value;
            UpdateTitleLabelText($"{LandOnPropertyName}");
            UpdatePriceLabelText($"{LandOnPropertyPrice}");
            Show();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            OnAcceptButton = () => { 
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventsEnum.Purchase });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    // This behaves more like a controller, determining what panels to show and changing their content.
    public class PropertyPanel : OnLandPanel
    {
        private readonly YouBoughtPanel youBoughtPanel; 
        private readonly PayRentPanel payRentPanel;
        private readonly BuyPanel buyPanel;
        private readonly UpgradeHousePanel upgradeHousePanel;
        private ShowPanelContext showPanelContext;

        public PropertyPanel(VisualElement parent) : base(parent)
        {
            PanelType = SpaceTypeEnum.Property;
            youBoughtPanel = new(parent);
            payRentPanel = new(parent);
            buyPanel = new(parent);
            upgradeHousePanel = new(parent);
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
            if (!IsSpaceFree(ownerID) && IsPlayerOwner(ownerID, context.playerID))
            {
                upgradeHousePanel.Show(context);
            }
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            buyPanel.AddAcceptButtonAction(entityQuery);
            payRentPanel.AddAcceptButtonAction(entityQuery);
            youBoughtPanel.AddAcceptButtonAction(entityQuery);
            upgradeHousePanel.AddAcceptButtonAction(entityQuery);
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
            youBoughtPanel.Show(showPanelContext);
        }

        public override void Dispose()
        {
            base.Dispose();
            buyPanel.AcceptButton.clicked -= OnBuyPanelAcceptClicked;
            buyPanel.Dispose();
            payRentPanel.Dispose();
            youBoughtPanel.Dispose();
            upgradeHousePanel.Dispose();
        }
    }
}
