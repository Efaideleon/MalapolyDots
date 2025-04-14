using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels; // For Panel classes - Assuming this namespace is correct
using System; // Needed for System.Action if using callback approach

[TestFixture]
public class GameUICanvasSystemTests
{
    private World _previousWorld;
    private World _testWorld;
    private EntityManager _entityManager;

    private Entity _playerEntity;
    private Entity _propertyEntity;
    private Entity _gameStateEntity;       // Handle for the GameState singleton entity
    private Entity _currentPlayerIdEntity; // Handle for the CurrentPlayerID singleton entity
    private Entity _landedOnSpaceEntity;   // Handle for the LandedOnSpace singleton entity
    private Entity _transactionEventsEntity; // Handle for the TransactionEvents singleton entity
    private Entity _canvasRefEntity;
    private GameObject _uiPrefab;

    private const int VACANT_OWNER_ID = -1;
    private const int TEST_PLAYER_ID = 1;

    // Helper struct to return results from GetUIElements
    private struct UIElementsResult
    {
        public UIDocument Document;
        public VisualElement Container; // The 'game-screen-bottom-container'
    }

    [SetUp]
    public void Setup()
    {
        _previousWorld = World.DefaultGameObjectInjectionWorld;
        _testWorld = new World("TestWorld");
        World.DefaultGameObjectInjectionWorld = _testWorld;
        _entityManager = _testWorld.EntityManager;

        // --- SOLUTION 1: Explicitly Create the System ---
        // Ensures the system exists in this specific test world.
        // Use GetOrCreateSystemManaged for systems with managed components/data.
        SystemHandle systemHandle = _testWorld.GetOrCreateSystem<GameUICanvasSystem>();
        _testWorld.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(systemHandle);
        UnityEngine.Debug.Log(">>> Setup: Explicitly created/got GameUICanvasSystem <<<");

        _uiPrefab = Resources.Load<GameObject>("TestUI");
        Assert.IsNotNull(_uiPrefab, "Failed to load TestUI prefab from Resources.");
        var prefabDoc = _uiPrefab.GetComponent<UIDocument>();
        Assert.IsNotNull(prefabDoc, "TestUI prefab must have a UIDocument component.");
        Assert.IsNotNull(prefabDoc.visualTreeAsset, "UIDocument in prefab needs a Visual Tree Asset assigned.");

        _canvasRefEntity = _entityManager.CreateEntity();
        _entityManager.AddComponentObject(_canvasRefEntity, new CanvasReferenceComponent { uiDocumentGO = _uiPrefab });

        // Create Singletons using CreateSingleton where possible, otherwise CreateEntity + SetComponentData
        // Handles are stored in member variables
        _gameStateEntity = _entityManager.CreateSingleton<GameStateComponent>(new GameStateComponent { State = GameState.Rolling });
        _currentPlayerIdEntity = _entityManager.CreateSingleton<CurrentPlayerID>(new CurrentPlayerID { Value = TEST_PLAYER_ID });
        _landedOnSpaceEntity = _entityManager.CreateSingleton<LandedOnSpace>(new LandedOnSpace { entity = Entity.Null });

        // TransactionEvents needs special handling for the persistent queue
        _transactionEventsEntity = _entityManager.CreateEntity(typeof(TransactionEvents));
        _entityManager.SetComponentData(_transactionEventsEntity, new TransactionEvents
        {
            EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent)
        });


        _playerEntity = _entityManager.CreateEntity(typeof(PlayerID), typeof(NameComponent), typeof(MoneyComponent));
        _entityManager.SetComponentData(_playerEntity, new PlayerID { Value = TEST_PLAYER_ID });
        _entityManager.SetComponentData(_playerEntity, new NameComponent { Value = "TestPlayer" });
        _entityManager.SetComponentData(_playerEntity, new MoneyComponent { Value = 1500 });

        _propertyEntity = _entityManager.CreateEntity(
            typeof(SpaceTypeComponent), typeof(NameComponent), typeof(OwnerComponent),
            typeof(PriceComponent), typeof(RentComponent)
        );
        _entityManager.SetComponentData(_propertyEntity, new SpaceTypeComponent { Value = SpaceTypeEnum.Property });
        _entityManager.SetComponentData(_propertyEntity, new NameComponent { Value = "TestProperty" });
        _entityManager.SetComponentData(_propertyEntity, new PriceComponent { Value = 200 });
        _entityManager.SetComponentData(_propertyEntity, new RentComponent { Value = 20 });
        _entityManager.SetComponentData(_propertyEntity, new OwnerComponent { ID = VACANT_OWNER_ID });
    }

    [TearDown]
    public void TearDown()
    {
        if (_testWorld != null && _testWorld.IsCreated)
        {
            // Dispose the persistent queue using the stored entity handle
            // Check existence first in case TearDown is called after world disposal or entity destruction
            if (_entityManager.Exists(_transactionEventsEntity) && _entityManager.HasComponent<TransactionEvents>(_transactionEventsEntity))
            {
                var queue = _entityManager.GetComponentData<TransactionEvents>(_transactionEventsEntity).EventQueue;
                if (queue.IsCreated)
                {
                    queue.Dispose();
                }
            }

            // Use FindAnyObjectByType (newer API)
            var activeUIDocument = UnityEngine.Object.FindAnyObjectByType<UIDocument>();
            if (activeUIDocument != null && activeUIDocument.gameObject != null)
            {
                if (activeUIDocument.name.StartsWith(_uiPrefab.name))
                {
                    UnityEngine.Object.Destroy(activeUIDocument.gameObject);
                }
            }

            _testWorld.Dispose();
        }
        World.DefaultGameObjectInjectionWorld = _previousWorld;
    }

    // Updated Helper: Returns IEnumerator<object>, yielding nulls for waits
    // and the UIElementsResult struct as the *final* yielded value.
    private IEnumerator<object> GetUIElements()
    {
        // Wait for the system to potentially instantiate and update the UI
        yield return null; // Frame for system OnStartRunning/OnUpdate
        yield return null;

        // Find the UIDocument instance managed by the system (Use newer API)
        UIDocument uiDocument = UnityEngine.Object.FindAnyObjectByType<UIDocument>();
        Assert.IsNotNull(uiDocument, "Failed to find active UIDocument instance. Did the system instantiate it?");
        Assert.IsNotNull(uiDocument.rootVisualElement, "Found UIDocument, but rootVisualElement is null. UI Toolkit initialization incomplete?");

        // Wait another frame for UI Toolkit layout/styles to apply
        yield return null;

        // Now query elements
        var root = uiDocument.rootVisualElement;
        VisualElement gameScreenContainer = root.Q<VisualElement>("game-screen-bottom-container");
        Assert.IsNotNull(gameScreenContainer, "Could not find 'game-screen-bottom-container' in the active UI Document.");

        // Yield the final result object
        yield return new UIElementsResult { Document = uiDocument, Container = gameScreenContainer };
    }


    //[UnityTest]
    public IEnumerator UpgradeHousePanel_Appears_WhenPlayerOwnsProperty()
    {
        // --- Arrange ---
        // Set component data using the entity handle
        _entityManager.SetComponentData(_propertyEntity, new OwnerComponent { ID = TEST_PLAYER_ID });
        _entityManager.SetComponentData(_landedOnSpaceEntity, new LandedOnSpace { entity = _propertyEntity });
        _entityManager.SetComponentData(_gameStateEntity, new GameStateComponent { State = GameState.Landing });

        // --- Act ---
        // Run the helper coroutine and get the result
        _testWorld.Update();
        yield return null;
        _testWorld.Update();
        yield return null;
        var getElementsEnumerator = GetUIElements();
        yield return getElementsEnumerator; // Executes the steps within GetUIElements
        var uiResult = (UIElementsResult)getElementsEnumerator.Current; // Retrieve the final yielded result

        // --- Assert ---
        var rootContainer = uiResult.Container; // Use the container from the result
        var upgradePanel = rootContainer.Q<VisualElement>("UpgradeHousePanel");
        var buyPanel = rootContainer.Q<VisualElement>("PopupMenuPanel");
        var payRentPanel = rootContainer.Q<VisualElement>("PayRentPanel");
        // var youBoughtPanel = rootContainer.Q<VisualElement>("YouBoughtPanel"); // If needed

        Assert.IsNotNull(upgradePanel, "UpgradeHousePanel element not found.");
        Assert.IsNotNull(buyPanel, "BuyPanel (PopupMenuPanel) element not found.");
        Assert.IsNotNull(payRentPanel, "PayRentPanel element not found.");
        // Assert.IsNotNull(youBoughtPanel, "YouBoughtPanel element not found.");

        Assert.AreEqual(DisplayStyle.Flex, upgradePanel.resolvedStyle.display, "UpgradeHousePanel should be VISIBLE.");
        Assert.AreEqual(DisplayStyle.None, buyPanel.resolvedStyle.display, "BuyPanel should be HIDDEN.");
        Assert.AreEqual(DisplayStyle.None, payRentPanel.resolvedStyle.display, "PayRentPanel should be HIDDEN.");
        // Assert.AreEqual(DisplayStyle.None, youBoughtPanel.resolvedStyle.display, "YouBoughtPanel should be HIDDEN.");
    }

    //[UnityTest]
    public IEnumerator BuyPanel_Appears_WhenPropertyIsVacant()
    {
        // --- Arrange ---
        _entityManager.SetComponentData(_propertyEntity, new OwnerComponent { ID = VACANT_OWNER_ID });
        _entityManager.SetComponentData(_landedOnSpaceEntity, new LandedOnSpace { entity = _propertyEntity });
        _entityManager.SetComponentData(_gameStateEntity, new GameStateComponent { State = GameState.Landing });

        // --- Act ---
        _testWorld.Update();
        yield return null;
        _testWorld.Update();
        yield return null;
        var getElementsEnumerator = GetUIElements();
        yield return getElementsEnumerator;
        var uiResult = (UIElementsResult)getElementsEnumerator.Current;

        // --- Assert ---
        var rootContainer = uiResult.Container;
        var upgradePanel = rootContainer.Q<VisualElement>("UpgradeHousePanel");
        var buyPanel = rootContainer.Q<VisualElement>("PopupMenuPanel");
        var payRentPanel = rootContainer.Q<VisualElement>("PayRentPanel");

        Assert.IsNotNull(upgradePanel, "UpgradeHousePanel element not found.");
        Assert.IsNotNull(buyPanel, "BuyPanel (PopupMenuPanel) element not found.");
        Assert.IsNotNull(payRentPanel, "PayRentPanel element not found.");

        Assert.AreEqual(DisplayStyle.None, upgradePanel.resolvedStyle.display, "UpgradeHousePanel should be HIDDEN.");
        Assert.AreEqual(DisplayStyle.Flex, buyPanel.resolvedStyle.display, "BuyPanel should be VISIBLE.");
        Assert.AreEqual(DisplayStyle.None, payRentPanel.resolvedStyle.display, "PayRentPanel should be HIDDEN.");
    }

    //[UnityTest]
    public IEnumerator PayRentPanel_Appears_WhenPropertyOwnedByAnotherPlayer()
    {
        // --- Arrange ---
        const int OTHER_PLAYER_ID = 2;
        _entityManager.SetComponentData(_propertyEntity, new OwnerComponent { ID = OTHER_PLAYER_ID });
        _entityManager.SetComponentData(_landedOnSpaceEntity, new LandedOnSpace { entity = _propertyEntity });
        _entityManager.SetComponentData(_gameStateEntity, new GameStateComponent { State = GameState.Landing });

        // --- Act ---
        _testWorld.Update();
        yield return null;
        _testWorld.Update();
        yield return null;
        var getElementsEnumerator = GetUIElements();
        yield return getElementsEnumerator;
        var uiResult = (UIElementsResult)getElementsEnumerator.Current;

        // --- Assert ---
        var rootContainer = uiResult.Container;
        var upgradePanel = rootContainer.Q<VisualElement>("UpgradeHousePanel");
        var buyPanel = rootContainer.Q<VisualElement>("PopupMenuPanel");
        var payRentPanel = rootContainer.Q<VisualElement>("PayRentPanel");

        Assert.IsNotNull(upgradePanel, "UpgradeHousePanel element not found.");
        Assert.IsNotNull(buyPanel, "BuyPanel (PopupMenuPanel) element not found.");
        Assert.IsNotNull(payRentPanel, "PayRentPanel element not found.");

        Assert.AreEqual(DisplayStyle.None, upgradePanel.resolvedStyle.display, "UpgradeHousePanel should be HIDDEN.");
        Assert.AreEqual(DisplayStyle.None, buyPanel.resolvedStyle.display, "BuyPanel should be HIDDEN.");
        Assert.AreEqual(DisplayStyle.Flex, payRentPanel.resolvedStyle.display, "PayRentPanel should be VISIBLE.");
    }
}
