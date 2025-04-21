using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class TreasurePanel : OnLandPanel
    {
        public TreasurePanel(VisualElement parent) : base(parent.Q<VisualElement>("TreasurePanel"))
        {
            PanelType = SpaceType.Treasure;
            UpdateAcceptButtonReference("treasure-panel-button");
            UpdateLabelReference("treasure-panel-label");
            Hide();
        }

        // Move to the parent class and only call a class that will be overridden
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
