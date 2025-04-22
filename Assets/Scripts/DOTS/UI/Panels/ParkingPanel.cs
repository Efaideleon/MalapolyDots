using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class ParkingPanel : OnLandPanel
    {
        public ParkingPanel(VisualElement parent) : base(parent.Q<VisualElement>("ParkingPanel"))
        {
            PanelType = SpaceType.Parking;
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
        }
    }
}
