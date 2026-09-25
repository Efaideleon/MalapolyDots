using System;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.Mediator.Systems;
using DOTS.UI.Controllers;
using DOTS.UI.Panels;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class PropertyActionVerification
{
    [MenuItem("Tools/Malapoly/Verify Hotel and Rent Actions")]
    public static void Run()
    {
        using var world = new World("Property action verification");
        var em = world.EntityManager;
        var purchase = world.GetOrCreateSystem<PurchaseHousePanelUpdaterSystem>();
        var actions = world.GetOrCreateSystem<SpaceActionsPanelContextUpdaterSystem>();
        var buy = world.GetOrCreateSystem<BuyHouseSystem>();
        var game = em.CreateSingleton(new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true });
        var connection = em.CreateSingleton(new NetworkId { Value = 1 });
        var player = em.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(SpaceLandedOn), typeof(LandingPaymentResolved));
        em.SetComponentData(player, new GhostOwner { NetworkId = 1 });
        em.SetComponentData(player, new GhostMoneyComponet { Value = 100 });
        em.CreateSingleton(new CurrentActivePlayer { Entity = player });
        Entity Property(int id)
        {
            var p = em.CreateEntity(typeof(PropertySpaceTag), typeof(SpaceIDComponent), typeof(NameComponent),
                typeof(OwnerComponent), typeof(MonopolyFlagComponent), typeof(HouseCount), typeof(HousePriceComponent), typeof(ColorCodeComponent));
            em.SetComponentData(p, new SpaceIDComponent { Value = id });
            em.SetComponentData(p, new OwnerComponent { ID = 1 });
            em.SetComponentData(p, new MonopolyFlagComponent { Value = true });
            em.SetComponentData(p, new HouseCount { Value = 4 });
            em.SetComponentData(p, new HousePriceComponent { Value = 50 });
            em.SetComponentData(p, new ColorCodeComponent { Value = PropertyColor.Brown });
            return p;
        }
        var property = Property(1);
        var neighbor = Property(2);
        em.CreateSingleton(new LastPropertyInteracted { entity = property });
        em.SetComponentData(player, new SpaceLandedOn { entity = property });
        SpaceActionsPanelContext Refresh()
        {
            purchase.Update(world.Unmanaged);
            actions.Update(world.Unmanaged);
            using var query = em.CreateEntityQuery(typeof(SpaceActionsPanelContextComponent));
            return query.GetSingleton<SpaceActionsPanelContextComponent>().Value;
        }
        void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        var context = Refresh();
        Check(context.CanBuyHotel && !context.CanBuyHouse && !context.MustPayRent, "Four houses must enable only the hotel upgrade.");
        em.SetComponentData(player, new GhostMoneyComponet { Value = 49 });
        Check(!Refresh().CanBuyHotel, "Unaffordable hotel must be disabled.");
        em.SetComponentData(player, new GhostMoneyComponet { Value = 100 });
        em.SetComponentData(neighbor, new HouseCount { Value = 3 });
        Check(!Refresh().CanBuyHotel, "Uneven building must be disabled.");
        em.SetComponentData(neighbor, new HouseCount { Value = 4 });
        var rpc = em.CreateEntity(typeof(BuyHouseRpc), typeof(ReceiveRpcCommandRequest));
        em.SetComponentData(rpc, new BuyHouseRpc { PropertyId = 1, Count = 1 });
        em.SetComponentData(rpc, new ReceiveRpcCommandRequest { SourceConnection = connection });
        buy.Update(world.Unmanaged);
        Check(em.GetComponentData<HouseCount>(property).Value == 5 && em.GetComponentData<GhostMoneyComponet>(player).Value == 50,
            "Hotel purchase must upgrade four houses and charge once.");
        Check(!Refresh().CanBuyHotel && !Refresh().CanBuyHouse, "A hotel cannot be upgraded again.");
        em.SetComponentData(property, new OwnerComponent { ID = 2 });
        Check(Refresh().MustPayRent, "Landing on another player's property must show rent.");
        em.SetComponentData(player, new LandingPaymentResolved { Value = true });
        Check(!Refresh().MustPayRent, "Paid rent must be hidden.");
        em.SetComponentData(player, new LandingPaymentResolved());
        em.SetComponentData(property, new OwnerComponent { ID = PropertyConstants.Vacant });
        Check(!Refresh().MustPayRent, "Unowned property must not show rent.");
        em.AddComponentData(player, new PlayerMovementState { Value = MoveState.Idle });
        em.AddComponentData(property, new PriceComponent { Value = 50 });
        Check(Refresh().CanBuyProperty, "Exact funds on an unowned landed property must allow buying.");
        em.SetComponentData(player, new GhostMoneyComponet { Value = 49 });
        Check(!Refresh().CanBuyProperty, "Insufficient funds must disable property buying.");
        em.SetComponentData(player, new GhostMoneyComponet { Value = 50 });
        em.SetComponentData(property, new OwnerComponent { ID = 1 });
        Check(!Refresh().CanBuyProperty, "Own property must not be purchasable.");
        em.SetComponentData(property, new OwnerComponent { ID = 2 });
        Check(!Refresh().CanBuyProperty, "Another player's property must not be purchasable.");
        em.SetComponentData(property, new OwnerComponent { ID = PropertyConstants.Vacant });
        em.SetComponentData(player, new GhostOwner { NetworkId = 2 });
        Check(!Refresh().CanBuyProperty, "Another player's turn must disable property buying.");
        em.SetComponentData(player, new GhostOwner { NetworkId = 1 });
        em.SetComponentData(player, new SpaceLandedOn { entity = neighbor });
        Check(!Refresh().CanBuyProperty, "Only the landed property may be purchased.");
        em.SetComponentData(player, new SpaceLandedOn { entity = property });
        em.SetComponentData(player, new PlayerMovementState { Value = MoveState.Walking });
        Check(!Refresh().CanBuyProperty, "Walking must disable property buying.");
        var root = new VisualElement();
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/TitleScreenUI/SpaceActions.uxml").Instantiate();
        tree.name = "SpaceActions";
        root.Add(tree);
        var panel = new SpaceActionsPanel(root);
        panel.SetHotelPurchaseAvailability(true);
        panel.SetPropertyPurchaseAvailability(false);
        panel.Show();
        Check(panel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.enabledSelf, "Hotel button must be wired and enabled.");
        Check(panel.ButtonSet[SpaceActionButtonsEnum.PayRent].Button.style.display.value == DisplayStyle.None, "Rent button must start hidden.");
        panel.SetRentPaymentAvailability(true);
        Check(panel.ButtonSet[SpaceActionButtonsEnum.PayRent].Button.style.display.value == DisplayStyle.Flex, "Rent button must appear when owed.");
        Check(!panel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.enabledSelf, "Showing the menu must preserve disabled property buying.");
        panel.SetPropertyPurchaseAvailability(true);
        Check(panel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.enabledSelf, "Eligible property buying must be enabled.");
        panel.Hide();
        Check(!panel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.enabledSelf, "Hidden property buying must remain disabled.");
        Check(!panel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.enabledSelf, "Hidden menu buttons must be disabled.");
        Debug.Log("Property action verification PASS: hotel eligibility, affordability, even building, server purchase, rent ownership/payment, property purchase eligibility, and UXML button wiring.");
    }
}
