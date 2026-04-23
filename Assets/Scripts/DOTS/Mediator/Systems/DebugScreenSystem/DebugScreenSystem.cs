using System.Diagnostics;
using Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem;
using Unity.Entities;
using Unity.NetCode;
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

    internal class MonopolyToggleManagedData : IComponentData
    {
        public Toggle? Toggle;
        public EventCallback<ChangeEvent<bool>>? Callback;
    }


    internal class IntegerFieldManagedData : IComponentData
    {
        public IntegerField? IntegerField;
        public EventCallback<ChangeEvent<int>>? Callback;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
            state.EntityManager.AddComponentData(entity, new MonopolyToggleManagedData { Toggle = null, Callback = null });
            state.EntityManager.AddComponentData(entity, new IntegerFieldManagedData { IntegerField = null, Callback = null });

            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<ForegroundContainterComponent>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<RollConfig>();
            state.RequireForUpdate<GlobalMonopolyEnabled>();
            state.RequireForUpdate<ToggleCustomRollEventBuffer>();
            state.RequireForUpdate<CustomRollValueEventBuffer>();
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
            var globalMonopolyQuery = SystemAPI.QueryBuilder().WithAllRW<GlobalMonopolyEnabled>().Build();
            var debugScreenEntity = SystemAPI.GetSingletonEntity<DebugScreenFlag>();

            var toggleCustomRollQuery = SystemAPI.QueryBuilder().WithAllRW<ToggleCustomRollEventBuffer>().Build();
            var customRollValueQuery = SystemAPI.QueryBuilder().WithAllRW<CustomRollValueEventBuffer>().Build();

            SetupRollToggle(ref state, debugScreen, rollConfigQuery, ref debugScreenEntity, toggleCustomRollQuery);
            SetupRollIntegerField(ref state, debugScreen, rollConfigQuery, ref debugScreenEntity, customRollValueQuery);
            SetupGlobalMonopolyToggle(ref state, debugScreen, globalMonopolyQuery, ref debugScreenEntity);
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

            var monopolyToggle = SystemAPI.ManagedAPI.GetSingleton<MonopolyToggleManagedData>();
            monopolyToggle.Toggle?.UnregisterCallback(monopolyToggle.Callback);
        }

        private readonly void SetupRollToggle(
                ref SystemState state,
                VisualElement debugScreen,
                EntityQuery rollConfigQuery,
                ref Entity debugScreenEntity,
                EntityQuery toggleCustomRollQuery
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
                var toggleCustomRoll = toggleCustomRollQuery.GetSingletonBuffer<ToggleCustomRollEventBuffer>();
                toggleCustomRoll.Add(new ToggleCustomRollEventBuffer { Value = evt.newValue });
                UnityEngine.Debug.Log($"[DebugScreenSystem] | isCustomEnabled: {evt.newValue}");
            };

            toggleData.Toggle.RegisterCallback(toggleData.Callback);
        }

        private readonly void SetupRollIntegerField(
                ref SystemState state,
                VisualElement debugScreen,
                EntityQuery rollConfigQuery,
                ref Entity debugScreenEntity,
                EntityQuery customRollValueQuery
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
                var customRollValue = customRollValueQuery.GetSingletonBuffer<CustomRollValueEventBuffer>();
                customRollValue.Add(new CustomRollValueEventBuffer { Value = evt.newValue});
                UnityEngine.Debug.Log($"[DebugScreenSystem] | customRollValue: {evt.newValue}");
            };

            integerFieldData.IntegerField.RegisterCallback(integerFieldData.Callback);
        }

        private readonly void SetupGlobalMonopolyToggle(
                ref SystemState state,
                VisualElement debugScreen,
                EntityQuery globalMonopolyQuery,
                ref Entity debugScreenEntity
        )
        {
            var monopolyToggle = debugScreen.Q<Toggle>("GlobalMonopoly");
            if (monopolyToggle == null)
            {
                UnityEngine.Debug.LogWarning($"[DebugScreenSystem] | MonopolyToggle is missing.");
                return;
            }

            var toggleData = SystemAPI.ManagedAPI.GetComponent<MonopolyToggleManagedData>(debugScreenEntity);

            toggleData.Toggle = monopolyToggle;

            toggleData.Callback = (evt) =>
            {
                ref var globalMonopolyRW = ref globalMonopolyQuery.GetSingletonRW<GlobalMonopolyEnabled>().ValueRW;
                globalMonopolyRW.Value = evt.newValue;
                UnityEngine.Debug.Log($"[DebugScreenSystem] | GlobalMonopolyEnabled: {globalMonopolyRW.Value}");
            };

            toggleData.Toggle.RegisterCallback(toggleData.Callback);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DebugScreenProxyToServer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.EntityManager.CreateSingletonBuffer<ToggleCustomRollEventBuffer>();
            state.EntityManager.CreateSingletonBuffer<CustomRollValueEventBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ToggleCustomRollEventBuffer>>())
            {
                foreach (var value in buffer)
                {
                    var entity = ecb.CreateEntity();
                    ecb.AddComponent(entity, new ToggleCustomRollRpc { Value = value.Value });
                    ecb.AddComponent<SendRpcCommandRequest>(entity);
                    UnityEngine.Debug.Log($"[DebugScreenProxyToServer] | sending toggle rpc {value.Value}");
                }
                buffer.Clear();
            }

            foreach (var buffer in SystemAPI.Query<DynamicBuffer<CustomRollValueEventBuffer>>())
            {
                foreach (var value in buffer)
                {
                    var entity = ecb.CreateEntity();
                    ecb.AddComponent(entity, new CustomRollValueRpc { Value = value.Value });
                    ecb.AddComponent<SendRpcCommandRequest>(entity);
                    UnityEngine.Debug.Log($"[DebugScreenProxyToServer] | sending custom value rpc {value.Value}");
                }
                buffer.Clear();
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ToggleCustomRollEventBuffer : IBufferElementData
    {
        public bool Value;
    }

    public struct CustomRollValueEventBuffer : IBufferElementData
    {
        public int Value;
    }

    public struct ToggleCustomRollRpc : IRpcCommand
    { 
        public bool Value;
    }

    public struct CustomRollValueRpc : IRpcCommand
    { 
        public int Value;
    }
}
