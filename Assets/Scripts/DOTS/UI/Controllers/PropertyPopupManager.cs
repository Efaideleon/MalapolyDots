using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.UI.Panels;

namespace DOTS.UI.Controllers
{
    public struct PropertyPopupManagerContext
    {
        public int OwnerID;         
        public int CurrentPlayerID; 
        public bool isLocal;
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

            UnityEngine.Debug.Log($"[PropertyPopupManager] | currentPlayerID: {currentPlayerID}");
            UnityEngine.Debug.Log($"[PropertyPopupManager] | ownerID: {ownerID}");
            if (!IsSpaceFree(ownerID) && !IsPlayerOwner(ownerID, currentPlayerID) && Context.isLocal)
            {
                UnityEngine.Debug.Log($"[PropertyPopupManager] | showing pay rent panel");
                PayRentPanel.Show(); 
            }
        }

        private bool IsPlayerOwner(int ownerID, int playerID) => ownerID == playerID;
        private bool IsSpaceFree(int ownerID) => ownerID == PropertyConstants.Vacant;
    }
}
