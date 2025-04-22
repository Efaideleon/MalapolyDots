using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class GoPanel : OnLandPanel
    {
        public GoPanel(VisualElement parent) : base (parent.Q<VisualElement>("GoPanel"))
        {
            PanelType = SpaceType.Go;
            UpdateAcceptButtonReference("go-panel-button");
            UpdateLabelReference("go-panel-label");
            Hide();
        }

        public override void Show(ShowPanelContext context)
        {
            Show();
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
        }
    }
}
