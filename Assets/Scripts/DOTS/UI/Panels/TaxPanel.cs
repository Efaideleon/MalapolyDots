using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class TaxPanel : OnLandPanel
    {
        public TaxPanel(VisualElement parent) : base (parent.Q<VisualElement>("TaxPanel"))
        {
            PanelType = SpaceType.Tax;
            UpdateAcceptButtonReference("tax-panel-button");
            UpdateLabelReference("tax-panel-label");
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
