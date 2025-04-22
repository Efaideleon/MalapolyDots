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
        public RollPanelController rollPanelController;
        public ChangeTurnPanelController changeTurnPanelController;
        public StatsPanelController statsPanelController;
    }
}

