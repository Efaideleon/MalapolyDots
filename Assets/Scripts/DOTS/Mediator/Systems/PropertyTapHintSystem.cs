using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.Mediator.Systems;
using Input;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(SpaceActionsPanelPopupManagedSystem))]
    public partial class PropertyTapHintSystem : SystemBase
    {
        readonly PropertyTapGlow glow = new();
        PropertyTapPromptOverlay overlay;
        EntityQuery colliders;

        protected override void OnCreate()
        {
            EntityManager.CreateSingleton<PropertyTapHintState>();
            colliders = PropertyTapHintRules.CreateCollidersQuery(EntityManager);
            RequireForUpdate<NetworkStreamInGame>();
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<CurrentActivePlayer>();
            RequireForUpdate<NetworkId>();
            RequireForUpdate<ForegroundContainterComponent>();
            RequireForUpdate<GameScreenInitializedFlag>();
        }

        protected override void OnUpdate()
        {
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            bool debt = false;
            foreach (var (money, bankruptcy) in SystemAPI.Query<RefRO<GhostMoneyComponet>, RefRO<BankruptPlayer>>())
                debt |= !bankruptcy.ValueRO.Value && money.ValueRO.Value < 0;
            var hint = SystemAPI.GetSingleton<PropertyTapHintState>();
            hint.Property = PropertyTapHintRules.EligibleProperty(EntityManager, game,
                SystemAPI.GetSingleton<CurrentActivePlayer>().Entity, SystemAPI.GetSingleton<NetworkId>().Value, debt);
            hint.Hovered = false;
            hint.CanPrompt = false;
            if (SystemAPI.TryGetSingleton<LocalPropertyTapResult>(out var tap)) hint.ObserveTap(tap);
            var foreground = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>().Value;
            var camera = Camera.main;
            if (hint.Property == Entity.Null || foreground?.panel == null || camera == null)
            {
                overlay?.Hide();
                SystemAPI.SetSingleton(hint);
                return;
            }

            Dependency.Complete();
            hint.Hovered = IsHovered(hint.Property, camera);
            if (overlay == null)
            {
                var go = new GameObject("Property tap prompt") { hideFlags = HideFlags.DontSave };
                overlay = go.AddComponent<PropertyTapPromptOverlay>();
            }
            overlay.Bind(foreground);

            var property = hint.Property;
            var mesh = glow.GetMesh(EntityManager, property, out var matrix, out var anchor);
            if (mesh == null)
            {
                overlay.Hide();
                SystemAPI.SetSingleton(hint);
                return;
            }
            var screenAnchor = camera.WorldToScreenPoint(anchor);
            hint.CanPrompt = screenAnchor.z > camera.nearClipPlane && camera.pixelRect.Contains(screenAnchor) &&
                !TouchInputSystem.IsOverUI(screenAnchor);
            SystemAPI.SetSingleton(hint);
            if ((!hint.Learned && hint.CanPrompt) || hint.Hovered) glow.Draw(mesh, matrix, hint.Hovered, camera);
            overlay.Track(anchor, !hint.Learned && hint.CanPrompt, hint.Hovered);
        }

        bool IsHovered(Entity property, Camera camera)
        {
            var mouse = Mouse.current;
            if (mouse == null || !Application.isFocused || UnityEngine.Cursor.lockState != CursorLockMode.None) return false;
            var position = mouse.position.ReadValue();
            if (!camera.pixelRect.Contains(position) || TouchInputSystem.IsOverUI(position)) return false;
            var ray = camera.ScreenPointToRay(position);
            return PropertyTapHintRules.IsHovered(EntityManager, colliders, property,
                ray.origin, ray.origin + ray.direction * 1000);
        }

        protected override void OnStopRunning() => overlay?.Hide();
        protected override void OnDestroy()
        {
            if (overlay != null) Object.Destroy(overlay.gameObject);
            glow.Dispose();
        }
    }
}
