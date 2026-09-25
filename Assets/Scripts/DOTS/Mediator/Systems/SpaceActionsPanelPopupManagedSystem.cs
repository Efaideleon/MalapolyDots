using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Input;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PropertyClickSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpaceActionsPanelPopupManagedSystem : ISystem
    {
        private uint pendingTapTick;
        private Entity pendingProperty;
        private Entity pendingPlayer;
        private Entity openedProperty;
        private Entity openedPlayer;
        private int releaseFrame;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowActionsPanelBuffer>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<LastPropertyInteracted>();
            state.RequireForUpdate<LocalPropertyTapResult>();

            state.EntityManager.CreateSingleton<LastProcessedTick>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var gameState = SystemAPI.GetSingleton<GameStateComponent>().State;
            var landedProperty = SystemAPI.HasComponent<SpaceLandedOn>(player)
                ? SystemAPI.GetComponent<SpaceLandedOn>(player).entity : Entity.Null;
            var movement = SystemAPI.HasComponent<PlayerMovementState>(player)
                ? SystemAPI.GetComponent<PlayerMovementState>(player).Value : MoveState.Walking;
            bool canOpen = SystemAPI.HasComponent<PropertySpaceTag>(landedProperty) &&
                PropertyActionRules.CanOpen(gameState, movement, landedProperty, landedProperty);

            var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
            if (!service.TryGet<SpaceActionsPanelController>(out var actions) ||
                !service.TryGet<BackdropController>(out var backdrop)) return;

            if (actions.IsOpen && (!canOpen || openedPlayer != player || openedProperty != landedProperty))
            {
                actions.HidePanel();
                backdrop.HideBackdrop();
            }

            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowActionsPanelBuffer>>())
            {
                bool hasMatchingLanding = false;
                var landing = SystemAPI.GetSingleton<GameStateComponent>();
                foreach (var request in buffer)
                    if (request.Player == player && request.Property == landedProperty &&
                        request.LandingSequence == landing.LandingSequence && landing.LandingPlayer == player)
                        hasMatchingLanding = true;
                if (hasMatchingLanding && canOpen)
                {
                    SystemAPI.GetSingletonRW<LastPropertyInteracted>().ValueRW.entity = landedProperty;
                    actions.ShowPanel();
                    backdrop.ShowBackdrop();
                    openedProperty = landedProperty;
                    openedPlayer = player;
                }
                buffer.Clear();
            }

            var tap = SystemAPI.GetSingleton<LocalPropertyTapResult>();
            var last = SystemAPI.GetSingletonRW<LastProcessedTick>();
            if (tap.Sequence != 0 && tap.Sequence != last.ValueRO.Tick)
            {
                last.ValueRW.Tick = tap.Sequence;
                pendingTapTick = 0;
                if (!tap.BlockedByUI && canOpen && tap.Property == landedProperty)
                {
                    pendingTapTick = tap.Sequence;
                    pendingProperty = tap.Property;
                    pendingPlayer = player;
                    releaseFrame = -1;
                }
            }

            if (pendingTapTick == 0) return;
            if (!canOpen || pendingPlayer != player || pendingProperty != landedProperty)
            {
                pendingTapTick = 0;
                return;
            }
            if (SystemAPI.ManagedAPI.TryGetSingleton<InputActionsComponent>(out var input) &&
                input.Value.Touch.Press.IsPressed())
            {
                releaseFrame = -1;
                return;
            }
            if (releaseFrame < 0)
            {
                releaseFrame = UnityEngine.Time.frameCount;
                return;
            }
            if (UnityEngine.Time.frameCount <= releaseFrame) return;

            if (actions.IsOpen && openedProperty == pendingProperty)
            {
                actions.HidePanel();
                backdrop.HideBackdrop();
                UnityEngine.Debug.Log("[PropertyTap] Actions menu closed.");
            }
            else
            {
                SystemAPI.GetSingletonRW<LastPropertyClicked>().ValueRW.entity = pendingProperty;
                SystemAPI.GetSingletonRW<LastPropertyInteracted>().ValueRW.entity = pendingProperty;
                actions.ShowPanel();
                backdrop.ShowBackdrop();
                openedProperty = pendingProperty;
                openedPlayer = player;
                UnityEngine.Debug.Log("[PropertyTap] Actions menu opened for the landed property.");
            }
            pendingTapTick = 0;
        }
    }

    public struct LastProcessedTick : IComponentData
    {
        public uint Tick;
    }
}
