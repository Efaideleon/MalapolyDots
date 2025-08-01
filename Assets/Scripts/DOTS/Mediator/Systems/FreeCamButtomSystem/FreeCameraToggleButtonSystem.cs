using System;
using DOTS.GamePlay.CameraSystems;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator.Systems
{
    internal class ButtonManagedData : IComponentData
    {
        public Button Button;
        public Action Callback;
    }

    public struct FreeCameraButtonTag : IComponentData
    { }

    public partial struct FreeCameraToggleButtonSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<ButtonManagedData>(),
                ComponentType.ReadOnly<FreeCameraButtonTag>(),
            });
            state.EntityManager.AddComponentData(entity, new ButtonManagedData { Button = null, Callback = null});
            SystemAPI.SetComponent(entity, new FreeCameraButtonTag { });

            state.EntityManager.CreateSingleton(new FreeCameraToggleFlag { Value = false });
            state.RequireForUpdate<BotPanelRoot>();
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            // Getting the button
            var botPanelRoot = SystemAPI.ManagedAPI.GetSingleton<BotPanelRoot>().Value;
            if (botPanelRoot == null)
            {
                Debug.LogWarning($"[FreeCameraButtonSystem] | botPanelRoot is null. [{nameof(FreeCameraToggleButtonSystem)}]");
                return;
            }

            var button = botPanelRoot.Q<Button>("FreeCameraButton");
            if (button == null)
            {
                Debug.LogWarning($"[FreeCameraButtonSystem] | FreeCameraButton is missing. [{nameof(FreeCameraToggleButtonSystem)}]");
                return;
            }

            // Setting up button callback
            var freeCamToggleQuery = SystemAPI.QueryBuilder().WithAllRW<FreeCameraToggleFlag>().Build();
            var entity = SystemAPI.GetSingletonEntity<FreeCameraButtonTag>();
            var buttonData = SystemAPI.ManagedAPI.GetComponent<ButtonManagedData>(entity);

            buttonData.Callback = () =>
            {
                ref var state = ref freeCamToggleQuery.GetSingletonRW<FreeCameraToggleFlag>().ValueRW;
                state.Value = !state.Value;
                Debug.Log($"[FreeCameraToggleButtonSystem] | FreeCamButton clicked");
            };

            button.clickable.clicked += buttonData.Callback;
            buttonData.Button = button;
        }

        public void OnUpdate(ref SystemState state)
        { }

        public void OnStopRunning(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<FreeCameraButtonTag>();
            var buttonData = SystemAPI.ManagedAPI.GetComponent<ButtonManagedData>(entity);
            buttonData.Button.clickable.clicked -= buttonData.Callback;
        }
    }
}
