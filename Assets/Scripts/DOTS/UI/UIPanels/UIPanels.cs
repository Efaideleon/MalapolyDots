using UnityEngine.UIElements;
using Unity.Entities;
using System;
using Unity.VisualScripting;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    //     public class Panel 
    //     {
    //         public VisualElement Root { get; private set; }
    //         public Label TopLabel { get; private set; }
    //         public Button AcceptButton { get; private set; }
    //
    //         public Action OnAcceptButton = null;
    //
    //         public Panel(VisualElement root)
    //         {
    //             Root = root;
    //         }
    //
    //         public void UpdateLabelReference(string className)
    //         {
    //             TopLabel = Root.Q<Label>(className);
    //         }
    //
    //         public void UpdateAcceptButtonReference(string className)
    //         {
    //             AcceptButton = Root.Q<Button>(className);
    //         }
    //
    //         public void UpdateLabelText(string text)
    //         {
    //             TopLabel.text = text;
    //         }
    //
    //         public virtual void AddAcceptButtonAction(EntityQuery entityQuery) { }
    //
    //         private void UnsubscribeAcceptButton()
    //         {
    //             if (OnAcceptButton != null)
    //             {
    //                 UnityEngine.Debug.Log("Unsubscribing AcceptButton");
    //                 AcceptButton.clickable.clicked -= OnAcceptButton;
    //             }
    //         }
    //
    //         public virtual void Show() => Root.style.display = DisplayStyle.Flex;
    //         public virtual void Hide() => Root.style.display = DisplayStyle.None;
    //
    //         public virtual void Dispose() => UnsubscribeAcceptButton();
    //     }
    //
    //     public class OnLandPanel : Panel
    //     {
    //         public SpaceTypeEnum PanelType { get; protected set; }
    //         public OnLandPanel(VisualElement parent) : base(parent) { }
    //         public virtual void HandleTransaction(Entity entity, EntityManager entityManager) { }
    //     }
    //
    //     public class TopPanel
    //     {
    //         private readonly UIDocument uiDocument;
    //         public VisualElement Root { get; private set; }
    //
    //         public TopPanel(UIDocument uiDocument)
    //         {
    //             this.uiDocument = uiDocument;
    //             Root = this.uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
    //         }
    //     }
    //
    //     public class BotPanel
    //     {
    //         private readonly UIDocument uiDocument;
    //         public VisualElement Root { get; private set; }
    //
    //         public BotPanel(UIDocument uiDocument)
    //         {
    //             this.uiDocument = uiDocument;
    //             Root = this.uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
    //         }
    //     }
    //
    //     public class StatsPanel
    //     {
    //         public VisualElement Parent { get; private set; }
    //         public Label PlayerNameLabel { get; private set; }
    //         public Label PlayerMoneyLabel { get; private set; }
    //
    //         public StatsPanel(VisualElement parent)
    //         {
    //             Parent = parent;
    //             PlayerNameLabel = Parent.Q<Label>("player-name-label");
    //             PlayerMoneyLabel = Parent.Q<Label>("player-money-label");
    //         }
    //
    //         public void UpdatePlayerNameLabelText(string text)
    //         {
    //             PlayerNameLabel.text = text;
    //         }
    //
    //         public void UpdatePlayerMoneyLabelText(string text)
    //         {
    //             PlayerMoneyLabel.text = text;
    //         }
    //     }
    //
    //     public class RollPanel : Panel
    //     {
    //         public Button RollButton { get; private set; }
    //         public Label RollLabel { get; private set; }
    //
    //         public Action OnRollButton;
    //
    //         public RollPanel(VisualElement parent) : base(parent.Q<VisualElement>("RollPanel"))
    //         {
    //             RollButton = Root.Q<Button>("roll-button");
    //             RollLabel = Root.Q<Label>("roll-amount-label");
    //             Hide();
    //         }
    //
    //         public void AddActionToRollButton(Action action)
    //         {
    //             OnRollButton = action;
    //             RollButton.clickable.clicked += OnRollButton;
    //         }
    //
    //         private void UnsubscribeRollButton()
    //         {
    //             RollButton.clickable.clicked -= OnRollButton;
    //         }
    //
    //         public void UpdateRollLabel(string text)
    //         {
    //             RollLabel.text = text;
    //         }
    //
    //         public override void Show()
    //         {
    //             base.Show();
    //             UpdateRollLabel("0");
    //             ShowRollButton();
    //         }
    //
    //         public void HideRollButton() => RollButton.style.display = DisplayStyle.None;
    //         public void ShowRollButton() => RollButton.style.display = DisplayStyle.Flex;
    //
    //         public override void Dispose() => UnsubscribeRollButton();
    //     }
    //
    //     public class YouBoughtPanel : Panel
    //     {
    //         public YouBoughtPanel(VisualElement parent) : base(parent.Q<VisualElement>("YouBoughtPanel"))
    //         {
    //             Hide();
    //         }
    //     }
    //
    //     public class PropertyPanel : OnLandPanel
    //     {
    //         private Label PriceLabel;
    //
    //         public PropertyPanel(VisualElement parent) : base(parent.Q<VisualElement>("PopupMenuPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Property;
    //             UpdateAcceptButtonReference("popup-menu-accept-button");
    //             UpdateLabelReference("buy-popup-menu-label");
    //             SetPriceLabelReference("price-popup-menu-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             var price = entityManager.GetComponentData<SpacePriceComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             UpdatePriceLabel($"{price.Value}");
    //             Show();
    //         }
    //
    //         private void SetPriceLabelReference(string className)
    //         {
    //             PriceLabel = Root.Q<Label>(className);
    //         }
    //
    //         private void UpdatePriceLabel(string text)
    //         {
    //             PriceLabel.text = text;
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Property});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class TaxPanel : OnLandPanel
    //     {
    //         public TaxPanel(VisualElement parent) : base (parent.Q<VisualElement>("TaxPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Tax;
    //             UpdateAcceptButtonReference("tax-panel-button");
    //             UpdateLabelReference("tax-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Tax});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class JailPanel : OnLandPanel
    //     {
    //         public JailPanel(VisualElement parent) : base(parent.Q<VisualElement>("JailPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Jail;
    //             UpdateAcceptButtonReference("jail-panel-button");
    //             UpdateLabelReference("jail-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Jail});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class GoToJailPanel : OnLandPanel
    //     {
    //         public GoToJailPanel(VisualElement parent) : base(parent.Q<VisualElement>("GoToJailPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.GoToJail;
    //             UpdateAcceptButtonReference("go-to-jail-panel-button");
    //             UpdateLabelReference("go-to-jail-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.GoToJail});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class ChancePanel : OnLandPanel
    //     {
    //         public ChancePanel(VisualElement parent) : base(parent.Q<VisualElement>("ChancePanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Chance;
    //             UpdateAcceptButtonReference("chance-panel-button");
    //             UpdateLabelReference("chance-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Chance});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class GoPanel : OnLandPanel
    //     {
    //         public GoPanel(VisualElement parent) : base (parent.Q<VisualElement>("GoPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Go;
    //             UpdateAcceptButtonReference("go-panel-button");
    //             UpdateLabelReference("go-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Go});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class ParkingPanel : OnLandPanel
    //     {
    //         public ParkingPanel(VisualElement parent) : base(parent.Q<VisualElement>("ParkingPanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Parking;
    //             UpdateAcceptButtonReference("parking-panel-button");
    //             UpdateLabelReference("parking-panel-label");
    //             Hide();
    //         }
    //
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Parking});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
    //
    //     public class TreasurePanel : OnLandPanel
    //     {
    //         public TreasurePanel(VisualElement parent) : base(parent.Q<VisualElement>("TreasurePanel"))
    //         {
    //             PanelType = SpaceTypeEnum.Treasure;
    //             UpdateAcceptButtonReference("treasure-panel-button");
    //             UpdateLabelReference("treasure-panel-label");
    //             Hide();
    //         }
    //
    //         // Move to the parent class and only call a class that will be overridden
    //         public override void HandleTransaction(Entity entity, EntityManager entityManager)
    //         {
    //             var name = entityManager.GetComponentData<NameComponent>(entity);
    //             UpdateLabelText($"{name.Value}");
    //             Show();
    //         }
    //
    //         public override void AddAcceptButtonAction(EntityQuery entityQuery)
    //         {
    //             OnAcceptButton = () => { 
    //                 var eventQueue = entityQuery.GetSingletonRW<TransactionEvents>().ValueRW.EventQueue;
    //                 eventQueue.Enqueue(new TransactionEvent{ EventType = SpaceTypeEnum.Treasure});
    //                 Hide();
    //             };
    //             AcceptButton.clickable.clicked += OnAcceptButton;
    //         }
    //     }
}
