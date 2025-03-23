using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;

public struct TransactionEvent
{
    public SpaceTypeEnum EventType;
}

public struct TransactionEvents : IComponentData
{
    public NativeQueue<TransactionEvent> EventQueue;
}

public struct RollAmountComponent : IComponentData
{
    public int Amount;
}

public class UIPanels : IComponentData
{
    public TopPanel topPanel;
    public BotPanel botPanel;
    public StatsPanel statsPanel;
    public RollPanel rollPanel;
    public YouBoughtPanel youBoughtPanel;
}

public class OnLandPanelsDictionay : IComponentData
{
    public Dictionary<SpaceTypeEnum, OnLandPanel> Value;
}

public partial struct GameUICanvasSystem : ISystem
{
    // public void OnCreate(ref SystemState state)
    // {
    //     state.RequireForUpdate<CanvasReferenceComponent>();
    //     state.RequireForUpdate<GameStateComponent>();
    //     state.RequireForUpdate<CurrPlayerID>();
    //
    //     EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
    //     if (query.IsEmpty)
    //     {
    //         var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
    //         {
    //             ComponentType.ReadOnly<TransactionEvents>()
    //         });
    //         SystemAPI.SetComponent(entity, new TransactionEvents
    //         {
    //             EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent)
    //         });
    //         UnityEngine.Debug.Log($"Creating NativeQueue");
    //     }
    //     else 
    //     {
    //         var entity = query.GetSingletonEntity();
    //         var events = state.EntityManager.GetComponentData<TransactionEvents>(entity);
    //
    //         if (events.EventQueue.IsCreated)
    //         {
    //             events.EventQueue.Dispose();
    //             events.EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent);
    //             state.EntityManager.SetComponentData(entity, events);
    //             UnityEngine.Debug.Log($"Reinitializing NativeQueue");
    //         }
    //     }
    // }
    //
    // public void OnStartRunning(ref SystemState state)
    // {
    //     UnityEngine.Debug.Log("OnStartRunning from GameUICanvasSystem");
    //     var canvasRef = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();
    //     var uiDocument = UnityEngine.Object.Instantiate(canvasRef.uiDocumentGO).GetComponent<UIDocument>();
    //
    //     TopPanel topPanel = new(uiDocument);
    //     BotPanel botPanel = new(uiDocument);
    //     StatsPanel statsPanel = new(topPanel.Root);
    //     RollPanel rollPanel = new(botPanel.Root);
    //     YouBoughtPanel youBoughtPanel = new(botPanel.Root);
    //
    //     var uiPanels = new UIPanels
    //     {
    //         topPanel = topPanel,
    //         botPanel = botPanel,
    //         statsPanel = statsPanel,
    //         rollPanel = rollPanel,
    //         youBoughtPanel = youBoughtPanel,
    //     };
    //
    //     state.EntityManager.AddComponentObject(state.SystemHandle, uiPanels);
    //
    //     Dictionary<SpaceTypeEnum, OnLandPanel> onLandPanelsDictionary = new()
    //     {
    //         { SpaceTypeEnum.Property, new PropertyPanel(botPanel.Root) }, 
    //         { SpaceTypeEnum.Tax, new TaxPanel(botPanel.Root) },
    //         { SpaceTypeEnum.Jail, new JailPanel(botPanel.Root) },
    //         { SpaceTypeEnum.GoToJail, new GoToJailPanel(botPanel.Root) },
    //         { SpaceTypeEnum.Chance, new ChancePanel(botPanel.Root) },
    //         { SpaceTypeEnum.Go, new GoPanel(botPanel.Root) },
    //         { SpaceTypeEnum.Parking, new ParkingPanel(botPanel.Root) },
    //         { SpaceTypeEnum.Treasure, new TreasurePanel(botPanel.Root) },
    //     };
    //
    //     state.EntityManager.AddComponentObject(state.SystemHandle, new OnLandPanelsDictionay 
    //     { 
    //         Value = onLandPanelsDictionary 
    //     });
    //
    //     // Create RollAmountComponent
    //     var rollAmountEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
    //     {
    //         ComponentType.ReadOnly<RollAmountComponent>(),
    //     });
    //
    //     SystemAPI.SetComponent(rollAmountEntity, new RollAmountComponent { Amount = 0 });
    //
    //     // Button Actions
    //     var rollAmountComponent = SystemAPI.QueryBuilder().WithAllRW<RollAmountComponent>().Build();
    //     uiPanels.rollPanel.AddActionToRollButton(() =>
    //     {
    //         var valueRolled = UnityEngine.Random.Range(1, 6);
    //         rollAmountComponent.GetSingletonRW<RollAmountComponent>().ValueRW.Amount = valueRolled;
    //         uiPanels.rollPanel.UpdateRollLabel(valueRolled.ToString());
    //         uiPanels.rollPanel.HideRollButton();
    //     });
    //
    //     var transactionEvents = SystemAPI.QueryBuilder().WithAllRW<TransactionEvents>().Build();
    //     foreach (var onLandPanel in onLandPanelsDictionary.Values)
    //     {
    //         UnityEngine.Debug.Log("Subscribing panels onStartRunning GameUICanvasSystem");
    //         onLandPanel.AddAcceptButtonAction(transactionEvents);
    //     }
    // }
    //
    // public void OnUpdate(ref SystemState state)
    // {
    //     var uiPanels = SystemAPI.ManagedAPI.GetComponent<UIPanels>(state.SystemHandle);
    //
    //     foreach (var currPlayerID in SystemAPI.Query<RefRO<CurrPlayerID>>().WithChangeFilter<CurrPlayerID>())
    //     {
    //         foreach (var (playerID, nameComponent, moneyComponent) 
    //                 in SystemAPI.Query<
    //                 RefRO<PlayerID>, 
    //                 RefRO<NameComponent>,
    //                 RefRO<MoneyComponent>
    //                 >()) 
    //         {
    //             if (playerID.ValueRO.Value == currPlayerID.ValueRO.Value)
    //             {
    //                 uiPanels.statsPanel.UpdatePlayerNameLabelText(nameComponent.ValueRO.Value.ToString());
    //                 uiPanels.statsPanel.UpdatePlayerMoneyLabelText(moneyComponent.ValueRO.Value.ToString());
    //             }
    //         }
    //     }
    //
    //     foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
    //     {
    //         switch (gameState.ValueRO.State)
    //         {
    //             case GameState.Rolling:
    //                 uiPanels.rollPanel.Show();
    //                 break;
    //             case GameState.Transaction:
    //                 uiPanels.rollPanel.Hide();
    //                 var spaceLanded = SystemAPI.GetSingleton<LandedOnSpace>();
    //                 var onLandPanelsDict = SystemAPI
    //                     .ManagedAPI
    //                     .GetComponent<OnLandPanelsDictionay>(state.SystemHandle)
    //                     .Value;
    //                 var spaceLandedType = SystemAPI
    //                     .GetComponent<SpaceTypeComponent>(spaceLanded.entity)
    //                     .Value;
    //
    //                 var onLandPanel = onLandPanelsDict[spaceLandedType];
    //                 onLandPanel.HandleTransaction(spaceLanded.entity, state.EntityManager);
    //                 break;
    //         }
    //     }
    // }
    //
    // public void OnStopRunning(ref SystemState state) 
    // {
    //     UnityEngine.Debug.Log("OnStopRunning from GameUICanvasSystem");
    //     var uiPanels = state.EntityManager.GetComponentObject<UIPanels>(state.SystemHandle);
    //     uiPanels.rollPanel.Dispose();
    //
    //     // First unsubscribe the button.clicked event 
    //     var onLandPanelsDictionary = state
    //         .EntityManager
    //         .GetComponentData<OnLandPanelsDictionay>(state.SystemHandle)
    //         .Value;
    //
    //     foreach (var onLandPanel in onLandPanelsDictionary.Values)
    //     {
    //         onLandPanel.Dispose();
    //         UnityEngine.Debug.Log($"Disposing onLandPanel");
    //     }
    // }
    //
    // //Is there a chance that the OnDestroy method from a system runs before the OnStopRunning of another?
    // public void OnDestroy(ref SystemState state)
    // { 
    //     // Then free the TransactionEvents.EventQueue since no anonymous function has a handle to the NativeQueue.
    //     var query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
    //     foreach (var entity in query.ToEntityArray(Allocator.Temp))
    //     {
    //         var transactionEvent = state.EntityManager.GetComponentData<TransactionEvents>(entity);
    //         UnityEngine.Debug.Log($"transactionEvent lenght: {transactionEvent.EventQueue.Count} ");
    //         UnityEngine.Debug.Log($"transactionEvent exists : {transactionEvent.EventQueue.IsCreated}");
    //         if (transactionEvent.EventQueue.IsCreated)
    //         {
    //             UnityEngine.Debug.Log("Diposing NativeQueue");
    //             transactionEvent.EventQueue.Dispose();
    //             transactionEvent.EventQueue = default;
    //             state.EntityManager.SetComponentData(entity, transactionEvent);
    //         }
    //     }
    // }
}

