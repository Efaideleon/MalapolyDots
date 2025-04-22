using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class GoToJailPanel : OnLandPanel
    {
        public GoToJailPanel(VisualElement parent) : base(parent.Q<VisualElement>("GoToJailPanel"))
        {
            PanelType = SpaceType.GoToJail;
            UpdateAcceptButtonReference("go-to-jail-panel-button");
            UpdateLabelReference("go-to-jail-panel-label");
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
