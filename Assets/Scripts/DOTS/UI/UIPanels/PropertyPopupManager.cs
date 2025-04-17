namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct PropertyPopupManagerContext
    {
        public int OwnerID;         
        public int CurrentPlayerID; 
    }

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
