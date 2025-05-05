using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public class PanelControllers : IComponentData
    {
        public PurchaseHousePanelController purchaseHousePanelController;
        public SpaceActionsPanelController spaceActionsPanelController;
        public BackdropController backdropController;
        public PurchasePropertyPanelController purchasePropertyPanelController;
        public PayRentPanelController payRentPanelController;
        public PayTaxPanelController payTaxPanelController;
        public TreasurePanelController treasurePanelController;
        public ChancePanelController chancePanelController;
        public JailPanelController jailPanelController;
        public ParkingPanelController parkingPanelController;
        public GoToJailPanelController goToJailPanelController;
        public GoPanelController goPanelController;
        public RollPanelController rollPanelController;
        public ChangeTurnPanelController changeTurnPanelController;
        public StatsPanelController statsPanelController;
    }
}

