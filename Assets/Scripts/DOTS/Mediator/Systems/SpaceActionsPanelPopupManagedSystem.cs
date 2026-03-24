using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpaceActionsPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowActionsPanelBuffer>();
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<ClickedPropertyComponent>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<UITappedPropertyEvent>();

            state.EntityManager.CreateSingleton<LastProcessedTick>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowActionsPanelBuffer>>().WithChangeFilter<ShowActionsPanelBuffer>())
            {
                if (buffer.Length > 0)
                {
                    UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | showing space actions.");
                    var panelService = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                    if (panelService.TryGet<SpaceActionsPanelController>(out var spaceActionsPanelController))
                    {
                        if (panelService.TryGet<BackdropController>(out var backdropController))
                        {
                            UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | Show space actions panels");
                            spaceActionsPanelController.ShowPanel();
                            backdropController.ShowBackdrop();
                        }
                    }
                }
                buffer.Clear();
            }

            foreach (var tappedPropertyEvent in SystemAPI.Query<RefRW<UITappedPropertyEvent>>().WithAll<GhostOwnerIsLocal, ActivePlayer>())
            {
                var lastProcessedTick = SystemAPI.GetSingletonRW<LastProcessedTick>();
                if (tappedPropertyEvent.ValueRO.EventTick > lastProcessedTick.ValueRO.Tick)
                {
                    var panelService = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                    if (panelService.TryGet<SpaceActionsPanelController>(out var spaceActionsPanelController))
                    {
                        if (panelService.TryGet<BackdropController>(out var backdropController))
                        {
                            Entity tappedProperty = tappedPropertyEvent.ValueRO.entity;
                            if (tappedProperty != Entity.Null)
                            {
                                if (SystemAPI.HasComponent<NameComponent>(tappedProperty))
                                {
                                    var name = SystemAPI.GetComponent<NameComponent>(tappedProperty);

                                    UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | Entity Hit: {name.Value}");
                                }
                                SystemAPI.GetSingletonRW<LastPropertyClicked>().ValueRW.entity = tappedProperty;

                                if (SystemAPI.HasComponent<PropertySpaceTag>(tappedProperty))
                                {
                                    spaceActionsPanelController.ShowPanel();
                                    backdropController.ShowBackdrop();
                                    UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | Processing the new event new tick: {tappedPropertyEvent.ValueRO.EventTick}, old tick: {lastProcessedTick.ValueRO.Tick}");
                                }
                            }
                        }
                    }
                    lastProcessedTick.ValueRW.Tick = tappedPropertyEvent.ValueRO.EventTick;
                }
            }
        }
    }

    public struct LastProcessedTick : IComponentData
    {
        public uint Tick;
    }
}
