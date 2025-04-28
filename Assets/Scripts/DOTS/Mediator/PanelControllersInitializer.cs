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

namespace DOTS.Mediator
{
    public class SpriteRegistryComponent : IComponentData
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

            state.EntityManager.CreateSingleton(new PanelControllers { purchaseHousePanelController = null, spaceActionsPanelController = null });
            state.EntityManager.CreateSingleton(new PopupManagers { propertyPopupManager = null });
            state.EntityManager.CreateSingleton(new SpriteRegistryComponent { Value = null });
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
            var uiDocument = uiGameObject.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }

            // Registering the properties sprites to their respective name
            Dictionary<FixedString64Bytes, Sprite> spaceSpriteRegistry = new();
            var sprites = canvasRef.spaceSprites;

            UnityEngine.Debug.Log("Loading sprite dictionary");
            for (int i = 0; i < sprites.Length; i++)
            {
                spaceSpriteRegistry.TryAdd(sprites[i].name, sprites[i]);
                UnityEngine.Debug.Log($"{sprites[i].name}");
            }
            SystemAPI.ManagedAPI.GetSingleton<SpriteRegistryComponent>().Value = spaceSpriteRegistry;

            var topPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
            var botPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
            var backdrop = uiDocument.rootVisualElement.Q<Button>("Backdrop");
            var playerNameMoneyContainer = botPanelRoot.Q<VisualElement>("PlayersStatsContainer");


            PurchasePropertyPanelContext purchasePropertyPanelContext = new() { Name = default, Price = default, };
            PurchaseHousePanelContext purchaseHousePanelContext = new() { Name = default, HousesOwned = default, Price = default };
            SpaceActionsPanelContext spaceActionsPanelContext = new() { HasMonopoly = false, IsPlayerOwner = false };
            PayRentPanelContext payRentPanelContext = new() { Rent = default };

            var tree = Resources.Load<VisualTreeAsset>("PlayerNameMoneyPanel");
            var numOfRounds = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;
            PlayerNameMoneyPanel[] playerNameMoneyPanels = new PlayerNameMoneyPanel[numOfRounds];
            for (int i = 0; i < numOfRounds; i++)
            {
                VisualElement playerNameMoneyPanelElement = tree.Instantiate();
                playerNameMoneyContainer.Add(playerNameMoneyPanelElement);
                playerNameMoneyPanels[i] = new PlayerNameMoneyPanel(playerNameMoneyPanelElement);
            }

            RollPanelContext rollPanelContext = new();
            ChangeTurnPanelContext changeTurnPanelContext = new();
            PlayerNameMoneyPanel statsPanel = new(botPanelRoot);
            RollPanel rollPanel = new(botPanelRoot);
            SpaceActionsPanel spaceActionsPanel = new(botPanelRoot);
            PurchaseHousePanel purchaseHousePanel = new(botPanelRoot, purchaseHousePanelContext);
            NoMonopolyYetPanel noMonopolyYetPanel = new(botPanelRoot);
            PurchasePropertyPanel purchasePropertyPanel = new(botPanelRoot);
            PayRentPanel payRentPanel = new(botPanelRoot);
            ChangeTurnPanel changeTurnPanel = new(botPanelRoot);

            // Loading Controllers
            var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

            panelControllers.backdropController = new(backdrop);
            panelControllers.backdropController.RegisterPanelToHide(spaceActionsPanel.Panel);
            panelControllers.backdropController.RegisterPanelToHide(purchaseHousePanel.Panel);
            panelControllers.backdropController.RegisterPanelToHide(noMonopolyYetPanel.Panel);
            panelControllers.backdropController.RegisterPanelToHide(purchasePropertyPanel.Panel);
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

            panelControllers.statsPanelController = new(statsPanel, new StatsPanelContext());
            panelControllers.payRentPanelController = new(payRentPanel, payRentPanelContext);
            panelControllers.rollPanelController = new(rollPanel, rollPanelContext);
            panelControllers.changeTurnPanelController = new(changeTurnPanel, changeTurnPanelContext);
            panelControllers.spaceActionsPanelController = new(
                    spaceActionsPanelContext,
                    spaceActionsPanel,
                    panelControllers.purchaseHousePanelController,
                    noMonopolyYetPanel,
                    panelControllers.purchasePropertyPanelController);

            PropertyPopupManagerContext propertyPopupManagerContext = new()
            {
                OwnerID = default,
                CurrentPlayerID = default
            };
            PropertyPopupManager propertyPopupManager = new(payRentPanel, propertyPopupManagerContext);
            SystemAPI.ManagedAPI.GetSingleton<PopupManagers>().propertyPopupManager = propertyPopupManager;

            // Button Actions
            var rollEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<RollEventBuffer>().Build();
            var transactionEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEventBuffer>().Build();
            var buyHouseEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEventBuffer>().Build();
            panelControllers.purchaseHousePanelController.SetEventBufferQuery(buyHouseEventBufferQuery);
            panelControllers.purchasePropertyPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.payRentPanelController.SetEventBufferQuery(transactionEventBufferQuery);
            panelControllers.rollPanelController.SetEventBufferQuery(rollEventBufferQuery);
            panelControllers.changeTurnPanelController.SetEventBufferQuery(transactionEventBufferQuery);
        }

        public void OnUpdate(ref SystemState state)
        { 
            state.Enabled = false;
        }

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
