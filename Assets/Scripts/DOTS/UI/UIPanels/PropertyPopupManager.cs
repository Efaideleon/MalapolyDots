using UnityEngine.UIElements;
using Unity.Entities;
using UnityEngine;

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

    //-------

    public struct PropertyPopupManagerContext
    {
        public int OwnerID;         
        public int CurrentPlayerID; 
    }

    // This behaves more like a controller, determining what panels to show and changing their content.
    // TODO: Rename This class
    public class PropertyPopupManager
    {
        public PayRentPanel PayRentPanel { get; private set; }
        public SpaceType SpacePanelType { get; private set; }
        public PropertyPopupManagerContext Context { get; set; }

        public PropertyPopupManager(PayRentPanel payRentPanel, PropertyPopupManagerContext context)
        {
            Context = context;
            SpacePanelType = SpaceType.Property;
            PayRentPanel = payRentPanel;
        }

        public void TriggerPopup()
        {
            var ownerID = Context.OwnerID;
            var currentPlayerID = Context.CurrentPlayerID;
            if (!IsSpaceFree(ownerID) && !IsPlayerOwner(ownerID, currentPlayerID)) 
                PayRentPanel.Show(); 
        }

        private bool IsPlayerOwner(int ownerID, int playerID) => ownerID == playerID;
        private bool IsSpaceFree(int ownerID) => ownerID == PropertyConstants.Vacant;
   }
}
