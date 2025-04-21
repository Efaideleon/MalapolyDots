using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class ChancePanel : OnLandPanel
    {
        public ChancePanel(VisualElement parent) : base(parent.Q<VisualElement>("ChancePanel"))
        {
            PanelType = SpaceType.Chance;
            UpdateAcceptButtonReference("chance-panel-button");
            UpdateLabelReference("chance-panel-label");
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
            OnAcceptButton = () => { 
                var eventBuffer = entityQuery.GetSingletonBuffer<TransactionEventBuffer>();
                eventBuffer.Add(new TransactionEventBuffer{ EventType = TransactionEventType.ChangeTurn });
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }
    }
}
