using Unity.Entities;
using DOTS.UI.Utilities;
using DOTS.SFX.Sound;
using DOTS.UI.Controllers;
using UnityEngine.UIElements;
using DOTS.UI.Panels;
using UnityEngine;
using DOTS.EventBuses;
using System.Collections.Generic;
using DOTS.Utilities.PropertiesBlob;
using Unity.Collections;
using DOTS.UI.Utilities.UIButtonEvents;

namespace DOTS.Mediator
{
    public class SpriteRegistryComponent : IComponentData
    {
        public Dictionary<FixedString64Bytes, Sprite> Value;
    }

    public class CharacterSpriteDictionary : IComponentData
    {
        public Dictionary<FixedString64Bytes, Sprite> Value;
    }

    public partial struct PanelControllersInitializer : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CanvasReferenceComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<PopupManagers>();
            state.RequireForUpdate<AudioSourceComponent>();
            state.RequireForUpdate<ClickSoundClipComponent>();
            state.RequireForUpdate<RollEventBuffer>();
            state.RequireForUpdate<BuyHouseEventBuffer>();
            state.RequireForUpdate<TransactionEventBuffer>();
            state.RequireForUpdate<PropertiesDataBlobReference>();
            state.RequireForUpdate<SpriteRegistryComponent>();
            state.RequireForUpdate<LoginData>();
            state.RequireForUpdate<UIButtonDirtyFlag>();
            state.RequireForUpdate<UIButtonEventBus>();
            state.EntityManager.CreateSingleton(new PanelControllers { purchaseHousePanelController = null, spaceActionsPanelController = null });
            state.EntityManager.CreateSingleton(new PopupManagers { propertyPopupManager = null });
            state.EntityManager.CreateSingleton(new SpriteRegistryComponent { Value = null });
            state.EntityManager.CreateSingleton(new CharacterSpriteDictionary { Value = new() });
        }

        public void OnStartRunning(ref SystemState state)
        {
            UnityEngine.Debug.Log(">>> GameUICanvasSystem.OnStartRunning CALLED <<<");
            var canvasRef = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();
            if (canvasRef.uiDocumentGO == null)
            {
                return;
            }

            // --- Instantiate Prefab ---
            var uiGameObject = UnityEngine.Object.Instantiate(canvasRef.uiDocumentGO);
            Debug.Log("Instantiating GameUI UIToolkit Canvas");
            var uiDocument = uiGameObject.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.Log("Destroying GameUI UIToolkit Canvas");
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }
            
            // Registering the properties sprites to their respective name
            Dictionary<FixedString64Bytes, Sprite> spaceSpriteRegistry = new();
            Dictionary<FixedString64Bytes, Sprite> characterSpriteRegistry = new();
            var sprites = canvasRef.spaceSprites;
            var characterSprites = canvasRef.characterSprites;

            for (int i = 0; i < sprites.Length; i++)
                spaceSpriteRegistry.TryAdd(sprites[i].name, sprites[i]);

            for (int i = 0; i < characterSprites.Length; i++)
                characterSpriteRegistry.TryAdd(characterSprites[i].name, characterSprites[i]);

            SystemAPI.ManagedAPI.GetSingleton<CharacterSpriteDictionary>().Value = characterSpriteRegistry;
            SystemAPI.ManagedAPI.GetSingleton<SpriteRegistryComponent>().Value = spaceSpriteRegistry;

            var botPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
            var backdrop = uiDocument.rootVisualElement.Q<Button>("Backdrop");
            var playerNameMoneyContainer = botPanelRoot.Q<VisualElement>("PlayersStatsContainer");

            if (playerNameMoneyContainer == null)
                return;

            PurchasePropertyPanelContext purchasePropertyPanelContext = new() { Name = default, Price = default, };
            PurchaseHousePanelContext purchaseHousePanelContext = new() { Name = default, HousesOwned = default, Price = default };
            SpaceActionsPanelContext spaceActionsPanelContext = new() { HasMonopoly = false, IsPlayerOwner = false };
            PayRentPanelContext payRentPanelContext = new() { Rent = default };
            PayTaxPanelContext payTaxPanelContext  = new() { Amount = default };
            TreasurePanelContext treasurePanelContext  = new() { Title = "Treasure" };
            ChancePanelContext chancePanelContext = new() { Title = "Chance" };
            JailPanelContext jailPanelContext = new() { Title = "Jail" };
            ParkingPanelContext parkingPanelContext = new () { Title = "Parking" };
            GoToJailPanelContext goToJailPanelContext = new () { Title = "GoToJail" };
            GoPanelContext goPanelContext = new () { Title = "Go" };

            RollPanelContext rollPanelContext = new();
            ChangeTurnPanelContext changeTurnPanelContext = new();

            RollPanel rollPanel = new(botPanelRoot);
            SpaceActionsPanel spaceActionsPanel = new(botPanelRoot);
            PurchaseHousePanel purchaseHousePanel = new(botPanelRoot, purchaseHousePanelContext);
            NoMonopolyYetPanel noMonopolyYetPanel = new(botPanelRoot);
            PurchasePropertyPanel purchasePropertyPanel = new(botPanelRoot);
            PayRentPanel payRentPanel = new(botPanelRoot);
            ChangeTurnPanel changeTurnPanel = new(botPanelRoot);
            TaxPanel payTaxPanel = new(botPanelRoot);
            TreasurePanel treasurePanel = new(botPanelRoot);
            ChancePanel chancePanel = new(botPanelRoot);
            JailPanel jailPanel = new(botPanelRoot);
            ParkingPanel parkingPanel = new(botPanelRoot);
            GoToJailPanel goToJailPanel = new(botPanelRoot);
            GoPanel goPanel = new(botPanelRoot);

            // Loading Controllers
            var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

            panelControllers.backdropController = new(backdrop);
            panelControllers.backdropController.RegisterPanelToHide(purchaseHousePanel);
            panelControllers.backdropController.RegisterPanelToHide(noMonopolyYetPanel);
            panelControllers.backdropController.RegisterPanelToHide(purchasePropertyPanel);
            panelControllers.purchaseHousePanelController = new(purchaseHousePanel);
            panelControllers.purchasePropertyPanelController = new(
                    purchasePropertyPanel,
                    purchasePropertyPanelContext,
                    new ManagedPurchasePropertyPanelContext());

            // -- Loading Audio --
            // -- PurchasePropertyPanel --
            var audioSourceRef = SystemAPI.ManagedAPI.GetSingleton<AudioSourceComponent>();
            var clickSound = SystemAPI.ManagedAPI.GetSingleton<ClickSoundClipComponent>().Value;
            if (audioSourceRef.AudioSourceGO != null)
            {
                var audioSourceGO = UnityEngine.Object.Instantiate(audioSourceRef.AudioSourceGO);
                var audioSource = audioSourceGO.GetComponent<AudioSource>();
                panelControllers.purchasePropertyPanelController.SetAudioSource(audioSource);
                panelControllers.purchasePropertyPanelController.SetClickSound(clickSound);
            }

            // Instantiating the Stats Panel for each player
            panelControllers.statsPanelController = new(playerNameMoneyContainer, new StatsPanelContext(), characterSpriteRegistry);
            foreach (var characterBuffer in SystemAPI.Query<DynamicBuffer<CharacterSelectedNameBuffer>>())
            {
                foreach (var character in characterBuffer)
                {
                    var tree = Resources.Load<VisualTreeAsset>("PlayerNameMoneyPanel");
                    VisualElement playerNameMoneyPanelElement = tree.Instantiate();
                    PlayerNameMoneyPanel panel = new(playerNameMoneyPanelElement);
                    var characterName = character.Name.ToString();
                    panelControllers.statsPanelController.RegisterPanel(characterName, panel);
                }
            }

            var transactionEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEventBuffer>().Build();
            var hideBackDropEvent = new HideBackDropEvent(panelControllers.backdropController);

            var payRentTransactionEvent = new TransactionEvent(transactionEventBufferQuery, TransactionEventType.PayRent);
            var payRentButtonEvents = new List<IButtonEvent> { payRentTransactionEvent, hideBackDropEvent };
            panelControllers.payRentPanelController = new(payRentPanel, payRentPanelContext, payRentButtonEvents);

            var payTaxTransactionEvent = new TransactionEvent(transactionEventBufferQuery, TransactionEventType.PayTaxes);
            var payTaxButtonEvents = new List<IButtonEvent> { payTaxTransactionEvent, hideBackDropEvent };
            panelControllers.payTaxPanelController = new(payTaxPanel, payTaxPanelContext, payTaxButtonEvents);

            panelControllers.treasurePanelController = new(treasurePanel, treasurePanelContext);
            panelControllers.chancePanelController = new(chancePanel, chancePanelContext);
            panelControllers.jailPanelController = new(jailPanel, jailPanelContext);
            panelControllers.parkingPanelController = new(parkingPanel, parkingPanelContext);
            panelControllers.goToJailPanelController = new(goToJailPanel, goToJailPanelContext);
            panelControllers.goPanelController = new(goPanel, goPanelContext);
            panelControllers.rollPanelController = new(rollPanel, rollPanelContext);
            panelControllers.changeTurnPanelController = new(changeTurnPanel, changeTurnPanelContext);

            var setButtonUIFlagQuery = SystemAPI.QueryBuilder().WithAllRW<UIButtonEventBus>().Build();
            var setButtonUIFlagEvent = new SetUIButtonFlagEvent(setButtonUIFlagQuery);
            panelControllers.spaceActionsPanelController = new(
                    spaceActionsPanelContext,
                    spaceActionsPanel,
                    panelControllers.purchaseHousePanelController,
                    noMonopolyYetPanel,
                    panelControllers.purchasePropertyPanelController,
                    setButtonUIFlagEvent
                    );

            panelControllers.backdropController.RegisterController(panelControllers.spaceActionsPanelController);

            PropertyPopupManagerContext propertyPopupManagerContext = new()
            {
                OwnerID = default,
                CurrentPlayerID = default
            };
            PropertyPopupManager propertyPopupManager = new(payRentPanel, propertyPopupManagerContext);
            SystemAPI.ManagedAPI.GetSingleton<PopupManagers>().propertyPopupManager = propertyPopupManager;
            var backdropEntityQuery = SystemAPI.QueryBuilder().WithAllRW<BackDropEventBus>().Build();
            // Button Actions
            var rollEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<RollEventBuffer>().Build();
            var buyHouseEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEventBuffer>().Build();
            panelControllers.purchaseHousePanelController.SetEventBufferQuery(buyHouseEventBufferQuery);
            panelControllers.purchasePropertyPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.treasurePanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.chancePanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.jailPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.parkingPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.goToJailPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.goPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.rollPanelController.SetEventBufferQuery(rollEventBufferQuery);
            panelControllers.changeTurnPanelController.SetEventBufferQuery(transactionEventBufferQuery, backdropEntityQuery);
        }

        public void OnUpdate(ref SystemState state)
        { }

        public void OnStopRunning(ref SystemState state)
        {
            var panelsController = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            panelsController.purchasePropertyPanelController.Dispose();
            panelsController.purchaseHousePanelController.Dispose();
            panelsController.spaceActionsPanelController.Dispose();
            panelsController.payRentPanelController.Dispose();
            panelsController.rollPanelController.Dispose();
            panelsController.backdropController.Dispose();
        }
    }
}
