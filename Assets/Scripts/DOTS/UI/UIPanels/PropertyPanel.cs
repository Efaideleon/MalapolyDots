using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Collections;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public class PropertyPanel : OnLandPanel
    {
        private Label PriceLabel;
        private readonly YouBoughtPanel youBoughtPanel; 
        private FixedString64Bytes landOnPropertyName;
        private int landOnPropertyPrice;

        public PropertyPanel(VisualElement parent) : base(parent.Q<VisualElement>("PopupMenuPanel"))
        {
            PanelType = SpaceTypeEnum.Property;
            youBoughtPanel = new(parent);
            UpdateAcceptButtonReference("popup-menu-accept-button");
            UpdateLabelReference("buy-popup-menu-label");
            SetPriceLabelReference("price-popup-menu-label");
            Hide();
        }

        public override void HandleTransaction(Entity entity, EntityManager entityManager)
        {
            var name = entityManager.GetComponentData<NameComponent>(entity);
            var price = entityManager.GetComponentData<SpacePriceComponent>(entity);
            landOnPropertyName = name.Value;
            landOnPropertyPrice = price.Value;
            UpdateLabelText($"{landOnPropertyName}");
            UpdatePriceLabel($"{landOnPropertyPrice}");
            Show();
        }

        private void SetPriceLabelReference(string className)
        {
            PriceLabel = Root.Q<Label>(className);
        }

        private void UpdatePriceLabel(string text)
        {
            PriceLabel.text = text;
        }

        public override void AddAcceptButtonAction(EntityQuery entityQuery)
        {
            youBoughtPanel.AddAcceptButtonAction(entityQuery);
            OnAcceptButton = () => { 
                youBoughtPanel.UpdateLabelText($"You bought: {landOnPropertyName}");
                var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
                eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventsEnum.Purchase });
                youBoughtPanel.Show();
                Hide();
            };
            AcceptButton.clickable.clicked += OnAcceptButton;
        }

        public override void Dispose()
        {
            base.Dispose();
            youBoughtPanel.Dispose();
        }
    }
}
