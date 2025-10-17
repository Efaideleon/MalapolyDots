using DOTS.GamePlay.DebugAuthoring;
using Unity.Entities;
using UnityEngine.UIElements;

#nullable enable
namespace DOTS.Mediator.Systems.DebugScreenSystem
{
    public struct DebugScreenFlag : IComponentData
    { }

    internal class ToggleManagedData : IComponentData
    {
        public Toggle? Toggle;
        public EventCallback<ChangeEvent<bool>>? Callback;
    }

    internal class IntegerFieldManagedData : IComponentData
    {
        public IntegerField? IntegerField;
        public EventCallback<ChangeEvent<int>>? Callback;
    }

    public partial struct DebugScreenSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<DebugScreenFlag>(),
                ComponentType.ReadOnly<ToggleManagedData>(),
                ComponentType.ReadOnly<IntegerFieldManagedData>(),
            });

            SystemAPI.SetComponent(entity, new DebugScreenFlag { });
            state.EntityManager.AddComponentData(entity, new ToggleManagedData { Toggle = null, Callback = null });
            state.EntityManager.AddComponentData(entity, new IntegerFieldManagedData { IntegerField = null, Callback = null });

            state.RequireForUpdate<ForegroundContainterComponent>();
            state.RequireForUpdate<DebugScreenFlag>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<RollConfig>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var foregroundContainterComponent = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>();
            var foregroundContainer = foregroundContainterComponent.Value;
            if (foregroundContainer == null)
            {
                UnityEngine.Debug.LogWarning($"[DebugScreenSystem] | foreground in GameScreen.uxml is missing.");
                return;
            }

            var debugScreen = foregroundContainer.Q<VisualElement>("DebugScreen"); 
            if (debugScreen == null)
            {
                UnityEngine.Debug.LogWarning($"[DebugScreenSystem] | DebugScreen in GameScreen.uxml is missing.");
                return;
            }

            var rollConfigQuery = SystemAPI.QueryBuilder().WithAllRW<RollConfig>().Build();
            var debugScreenEntity = SystemAPI.GetSingletonEntity<DebugScreenFlag>();

            SetupRollToggle(ref state, debugScreen, rollConfigQuery, debugScreenEntity);
            SetupRollIntegerField(ref state, debugScreen, rollConfigQuery, debugScreenEntity);
        }

        public void OnUpdate(ref SystemState state)
        { }

        public void OnStopRunning(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<DebugScreenFlag>();

            var toggleButton = SystemAPI.ManagedAPI.GetSingleton<ToggleManagedData>();
            toggleButton.Toggle?.UnregisterCallback(toggleButton.Callback);

            var integerFieldData = SystemAPI.ManagedAPI.GetSingleton<IntegerFieldManagedData>();
            integerFieldData.IntegerField?.UnregisterCallback(integerFieldData.Callback);
        }

        private readonly void SetupRollToggle(
                ref SystemState state,
                VisualElement debugScreen,
                EntityQuery rollConfigQuery,
                Entity debugScreenEntity
        )
        {
            var customRollToggle = debugScreen.Q<Toggle>("CustomRollToggle");
            if (customRollToggle == null)
            {
                UnityEngine.Debug.LogWarning($"[DebugScreenSystem] | customRollToggle is missing.");
                return;
            }

            var toggleData = SystemAPI.ManagedAPI.GetComponent<ToggleManagedData>(debugScreenEntity);

            toggleData.Toggle = customRollToggle;

            toggleData.Callback = (evt) =>
            {
                ref var rollConfigRW = ref rollConfigQuery.GetSingletonRW<RollConfig>().ValueRW;
                rollConfigRW.isCustomEnabled = evt.newValue;
                UnityEngine.Debug.Log($"[DebugScreenSystem] | isCustomEnabled: {rollConfigRW.isCustomEnabled}");
            };

            toggleData.Toggle.RegisterCallback(toggleData.Callback);
        }

        private readonly void SetupRollIntegerField(
                ref SystemState state,
                VisualElement debugScreen,
                EntityQuery rollConfigQuery,
                Entity debugScreenEntity
        )
        {
            var customRollIntegerField = debugScreen.Q<IntegerField>("CustomRollIntegerField");
            if (customRollIntegerField == null)
            {
                UnityEngine.Debug.LogWarning($"[DebugScreenSystem] | CustomRollIntegerField is missing.");
                return;
            }

            var integerFieldData = SystemAPI.ManagedAPI.GetComponent<IntegerFieldManagedData>(debugScreenEntity);

            integerFieldData.IntegerField = customRollIntegerField;

            integerFieldData.Callback = (evt) =>
            {
                ref var rollConfigRW = ref rollConfigQuery.GetSingletonRW<RollConfig>().ValueRW;
                rollConfigRW.customRollValue = evt.newValue;
                UnityEngine.Debug.Log($"[DebugScreenSystem] | customRollValue: {rollConfigRW.customRollValue}");
            };

            integerFieldData.IntegerField.RegisterCallback(integerFieldData.Callback);
        }
    }
}
