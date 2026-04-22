// using System;
// using Assets.Scripts.DOTS.Characters;
// using Assets.Scripts.DOTS.GamePlay;
// using DOTS.GamePlay.CameraSystems;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace DOTS.Mediator.Systems.FreeCamButtomSystem
// {
//     public struct RotateCameraButtonTag : IComponentData
//     { }
//
//     [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
//     public partial struct RotateCameraButtonSystem : ISystem, ISystemStartStop
//     {
//         const float RotationStepDegrees = 90f;
//
//         public void OnCreate(ref SystemState state)
//         {
//             var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
//             {
//                 ComponentType.ReadOnly<ButtonManagedData>(),
//                 ComponentType.ReadOnly<RotateCameraButtonTag>(),
//             });
//             state.EntityManager.AddComponentData(entity, new ButtonManagedData { Button = null, Callback = null });
//             SystemAPI.SetComponent(entity, new RotateCameraButtonTag { });
//
//             state.RequireForUpdate<BotPanelRoot>();
//             state.RequireForUpdate<GameScreenInitializedFlag>();
//             state.RequireForUpdate<PivotRotation>();
//             state.RequireForUpdate<PivotTransformTag>();
//             state.RequireForUpdate<CurrentActivePlayer>();
//         }
//
//         public void OnStartRunning(ref SystemState state)
//         {
//             var botPanelRoot = SystemAPI.ManagedAPI.GetSingleton<BotPanelRoot>().Value;
//             if (botPanelRoot == null)
//             {
//                 Debug.LogWarning($"[RotateCameraButtonSystem] | botPanelRoot is null. [{nameof(RotateCameraButtonSystem)}]");
//                 return;
//             }
//
//             var button = botPanelRoot.Q<Button>("RotateCameraButton");
//             if (button == null)
//             {
//                 Debug.LogWarning($"[RotateCameraButtonSystem] | RotateCameraButton is missing. [{nameof(RotateCameraButtonSystem)}]");
//                 return;
//             }
//
//             var pivotRotationQuery = SystemAPI.QueryBuilder().WithAllRW<PivotRotation>().Build();
//             var currentActivePlayerQuery = SystemAPI.QueryBuilder().WithAll<CurrentActivePlayer>().Build();
//             var angleAnimationQuery = SystemAPI.QueryBuilder().WithAll<AngleAnimationStateComponent>().Build();
//             var entityManager = state.EntityManager;
//             var entity = SystemAPI.GetSingletonEntity<RotateCameraButtonTag>();
//             var buttonData = SystemAPI.ManagedAPI.GetComponent<ButtonManagedData>(entity);
//
//             buttonData.Callback = () =>
//             {
//                 if (pivotRotationQuery.IsEmptyIgnoreFilter || currentActivePlayerQuery.IsEmptyIgnoreFilter)
//                     return;
//
//                 ref var pivotRotation = ref pivotRotationQuery.GetSingletonRW<PivotRotation>().ValueRW;
//                 var activePlayer = currentActivePlayerQuery.GetSingleton<CurrentActivePlayer>().Entity;
//                 if (activePlayer == Entity.Null || !entityManager.HasComponent<CurrentPivotRotation>(activePlayer))
//                     return;
//
//                 var rotationStep = quaternion.AxisAngle(math.up(), math.radians(RotationStepDegrees));
//                 var nextRotation = math.normalize(math.mul(pivotRotation.Value, rotationStep));
//
//                 pivotRotation.Value = nextRotation;
//                 entityManager.SetComponentData(activePlayer, new CurrentPivotRotation { Value = nextRotation });
//
//                 using var animationEntities = angleAnimationQuery.ToEntityArray(Allocator.Temp);
//                 foreach (var animationEntity in animationEntities)
//                 {
//                     entityManager.SetComponentData(animationEntity, new AngleAnimationStateComponent
//                     {
//                         Value = AngleAnimationState.Finished
//                     });
//                 }
//
//                 Debug.Log("[RotateCameraButtonSystem] | RotateCameraButton clicked");
//             };
//
//             button.clickable.clicked += buttonData.Callback;
//             buttonData.Button = button;
//         }
//
//         public void OnUpdate(ref SystemState state)
//         { }
//
//         public void OnStopRunning(ref SystemState state)
//         {
//             var entity = SystemAPI.GetSingletonEntity<RotateCameraButtonTag>();
//             var buttonData = SystemAPI.ManagedAPI.GetComponent<ButtonManagedData>(entity);
//             if (buttonData.Button != null && buttonData.Callback != null)
//             {
//                 buttonData.Button.clickable.clicked -= buttonData.Callback;
//             }
//         }
//     }
// }
