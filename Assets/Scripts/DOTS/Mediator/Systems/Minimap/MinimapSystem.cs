using System.Collections.Generic;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GamePlay.Minimap;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MinimapSystem : SystemBase
    {
        readonly List<MinimapPlayer> players = new();
        MinimapPanel panel;
        MinimapMapData map;
        bool reportedMissingMap;

        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<ForegroundContainterComponent>();
            RequireForUpdate<GameScreenInitializedFlag>();
            RequireForUpdate<NetworkId>();
        }

        protected override void OnUpdate()
        {
            var foreground = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>().Value;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            if (foreground == null || !game.AllPlacesInstantiated || game.State == GameState.GameOver)
            {
                ReleasePanel();
                return;
            }
            if (panel == null || panel.Root.panel == null || panel.Root.parent != foreground)
            {
                ReleasePanel();
                if (map == null) map = Resources.Load<MinimapMapData>("UI/MinimapMap");
                var asset = Resources.Load<VisualTreeAsset>("UI/Minimap");
                if (asset == null || map == null || !map.TryGetWorldBounds(out var bounds))
                {
                    if (!reportedMissingMap) Debug.LogWarning("Minimap needs its saved image and capture bounds in UI/MinimapMap.");
                    reportedMissingMap = true;
                    return;
                }
                var root = asset.CloneTree();
                root.AddToClassList("minimap-layer");
                root.pickingMode = PickingMode.Ignore;
                panel = new MinimapPanel(root, map, bounds);
                foreground.Insert(0, root);
            }
            players.Clear();
            foreach (var (transform, owner, name) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<GhostOwner>, RefRO<NameComponent>>().WithAll<CharacterFlag>())
            {
                var position = transform.ValueRO.Position;
                if (owner.ValueRO.NetworkId > 0 && float.IsFinite(position.x) && float.IsFinite(position.z))
                    players.Add(new MinimapPlayer(owner.ValueRO.NetworkId, name.ValueRO.Value, position));
            }
            int activeId = 0;
            if (SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active) && EntityManager.HasComponent<GhostOwner>(active.Entity))
                activeId = EntityManager.GetComponentData<GhostOwner>(active.Entity).NetworkId;
            var sprites = SystemAPI.ManagedAPI.TryGetSingleton<CharacterSpriteDictionary>(out var registry) ? registry.Value : null;
            panel.SetPlayers(players, SystemAPI.GetSingleton<NetworkId>().Value, activeId, sprites);
        }

        void ReleasePanel() { panel?.Dispose(); panel = null; }
        protected override void OnStopRunning() => ReleasePanel();
        protected override void OnDestroy() => ReleasePanel();
    }
}
