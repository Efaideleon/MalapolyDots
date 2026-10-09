using System;
using System.Collections.Generic;
using Assets.Scripts.DOTS.Mediator;
using DOTS.GamePlay.Minimap;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    public readonly struct MinimapPlayer
    {
        public readonly int NetworkId;
        public readonly FixedString64Bytes Name;
        public readonly Vector3 Position;

        public MinimapPlayer(int networkId, FixedString64Bytes name, Vector3 position)
        { NetworkId = networkId; Name = name; Position = position; }
    }

    sealed class MinimapPanel : IDisposable
    {
        sealed class Marker
        {
            public int Id, Number;
            public FixedString64Bytes Name;
            public Vector2 MapPosition, ScreenPosition;
            public VisualElement Root, Pin, Portrait;
            public Button Legend;
        }

        readonly Rect worldBounds;
        readonly MinimapViewport view;
        readonly Dictionary<int, Marker> markers = new();
        readonly List<Marker> ordered = new();
        readonly HashSet<int> seen = new();
        readonly List<int> removed = new();
        readonly VisualElement dock, viewport, image, markerLayer, legend;
        readonly Button collapse, expand, zoomIn, zoomOut, locate;
        readonly Label zoomLabel;
        readonly MinimapGestureManipulator gestures;
        bool collapsed, expanded;
        int nextNumber = 1, localId;

        public VisualElement Root { get; }

        public MinimapPanel(VisualElement root, MinimapMapData map, Rect bounds)
        {
            Root = root;
            worldBounds = bounds;
            view = new MinimapViewport(new Vector2(map.Image.width, map.Image.height));
            dock = root.Q("minimapDock");
            viewport = root.Q("minimapViewport");
            image = root.Q("minimapImage");
            markerLayer = root.Q("minimapMarkers");
            legend = root.Q("minimapLegend");
            collapse = root.Q<Button>("toggleMinimap");
            expand = root.Q<Button>("expandMinimap");
            zoomIn = root.Q<Button>("minimapZoomIn");
            zoomOut = root.Q<Button>("minimapZoomOut");
            locate = root.Q<Button>("minimapLocate");
            zoomLabel = root.Q<Label>("minimapZoomLevel");
            image.usageHints = UsageHints.DynamicTransform;
            image.style.backgroundImage = new StyleBackground(map.Image);
            image.style.width = map.Image.width;
            image.style.height = map.Image.height;
            gestures = new MinimapGestureManipulator(view, ApplyView);
            viewport.AddManipulator(gestures);
            viewport.RegisterCallback<GeometryChangedEvent>(Resize);
            root.RegisterCallback<GeometryChangedEvent>(AdaptLayout);
            collapse.clicked += Toggle;
            expand.clicked += ToggleExpanded;
            zoomIn.clicked += () => Zoom(1.35f);
            zoomOut.clicked += () => Zoom(1f / 1.35f);
            root.Q<Button>("minimapFit").clicked += () => { view.Fit(); ApplyView(); };
            locate.clicked += () => { if (markers.TryGetValue(localId, out var marker)) CenterOn(marker.MapPosition); };
            ApplyView();
        }

        public void SetPlayers(List<MinimapPlayer> players, int localNetworkId, int activeNetworkId, ISpriteRegistry sprites)
        {
            localId = localNetworkId;
            seen.Clear();
            ordered.Clear();
            players.Sort((a, b) => a.NetworkId.CompareTo(b.NetworkId));
            foreach (var player in players)
            {
                if (!seen.Add(player.NetworkId)) continue;
                if (!markers.TryGetValue(player.NetworkId, out var marker))
                {
                    marker = CreateMarker(player.NetworkId);
                    markers.Add(player.NetworkId, marker);
                }
                bool isLocal = player.NetworkId == localId;
                if (!marker.Name.Equals(player.Name) || marker.Legend.ClassListContains("is-you") != isLocal)
                {
                    marker.Name = player.Name;
                    string name = player.Name.ToString();
                    marker.Legend.text = marker.Number + "  " + name + (isLocal ? " · YOU" : "");
                    marker.Root.tooltip = name + (isLocal ? " (you)" : "");
                }
                if (marker.Portrait.style.backgroundImage.value.sprite == null &&
                    sprites != null && sprites.TryGet(player.Name, out var portrait))
                    marker.Portrait.style.backgroundImage = new StyleBackground(portrait);
                marker.Pin.EnableInClassList("is-you", isLocal);
                marker.Pin.EnableInClassList("is-current", player.NetworkId == activeNetworkId);
                marker.Legend.EnableInClassList("is-you", isLocal);
                marker.MapPosition = MinimapViewport.WorldToMap(player.Position, worldBounds);
                ordered.Add(marker);
            }
            removed.Clear();
            foreach (var pair in markers) if (!seen.Contains(pair.Key)) removed.Add(pair.Key);
            foreach (int id in removed)
            {
                markers[id].Root.RemoveFromHierarchy();
                markers[id].Legend.RemoveFromHierarchy();
                markers.Remove(id);
            }
            locate.SetEnabled(markers.ContainsKey(localId));
            UpdateMarkers();
            if (expanded) Root.BringToFront();
        }

        Marker CreateMarker(int id)
        {
            int number = nextNumber++;
            var marker = new Marker { Id = id, Number = number };
            string colorClass = "minimap-color-" + ((number - 1) % 6);
            marker.Root = new VisualElement { name = "minimapPlayer" + id, pickingMode = PickingMode.Ignore, usageHints = UsageHints.DynamicTransform };
            marker.Root.AddToClassList("minimap-marker");
            marker.Root.AddToClassList(colorClass);
            var anchor = new VisualElement { pickingMode = PickingMode.Ignore };
            anchor.AddToClassList("minimap-anchor");
            marker.Root.Add(anchor);
            marker.Pin = new Label(number.ToString()) { pickingMode = PickingMode.Ignore, usageHints = UsageHints.DynamicTransform };
            marker.Pin.AddToClassList("minimap-pin");
            marker.Root.Add(marker.Pin);
            markerLayer.Add(marker.Root);
            marker.Legend = new Button(() => CenterOn(marker.MapPosition)) { name = "minimapLocatePlayer" + id };
            marker.Legend.enableRichText = false;
            marker.Legend.AddToClassList("minimap-player");
            marker.Legend.AddToClassList(colorClass);
            var portrait = new VisualElement { pickingMode = PickingMode.Ignore };
            marker.Portrait = portrait;
            portrait.AddToClassList("minimap-portrait");
            marker.Legend.Add(portrait);
            legend.Add(marker.Legend);
            return marker;
        }

        void UpdateMarkers()
        {
            if (collapsed || view.Scale <= 0) return;
            foreach (var marker in ordered) marker.ScreenPosition = view.Project(marker.MapPosition);
            foreach (var marker in ordered)
            {
                int count = 0, index = 0;
                foreach (var other in ordered)
                    if ((marker.ScreenPosition - other.ScreenPosition).sqrMagnitude < 32f * 32f)
                    {
                        if (other.Id < marker.Id) index++;
                        count++;
                    }
                var offset = Vector2.zero;
                if (count > 1)
                {
                    float angle = -Mathf.PI * 0.5f + index * Mathf.PI * 2f / count;
                    offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (count > 3 ? 30f : 22f);
                }
                marker.Root.style.translate = new Translate(marker.ScreenPosition.x, marker.ScreenPosition.y);
                marker.Pin.style.translate = new Translate(offset.x - 16f, offset.y - 16f);
                marker.Root.EnableInClassList("is-outside", !new Rect(Vector2.zero, view.ViewportSize).Contains(marker.ScreenPosition));
                marker.Root.EnableInClassList("is-clustered", count > 1);
            }
        }

        void Resize(GeometryChangedEvent evt)
        {
            if (evt.newRect.width <= 0 || evt.newRect.height <= 0) return;
            view.Resize(viewport.contentRect.size);
            ApplyView();
        }

        void AdaptLayout(GeometryChangedEvent evt) => Root.EnableInClassList("is-small", evt.newRect.width < 1100 || evt.newRect.height < 700);

        void ApplyView()
        {
            var origin = view.Origin;
            image.style.translate = new Translate(origin.x, origin.y);
            image.style.scale = new Scale(new Vector3(view.Scale, view.Scale, 1f));
            zoomLabel.text = view.Zoom.ToString("0.0") + "×";
            zoomOut.SetEnabled(view.Zoom > 1.001f);
            zoomIn.SetEnabled(view.Zoom < MinimapViewport.MaxZoom - 0.001f);
            UpdateMarkers();
        }

        void Zoom(float factor) { view.ZoomAt(factor, view.ViewportSize * 0.5f); ApplyView(); }
        void CenterOn(Vector2 point)
        {
            if (view.Zoom < 2f) view.ZoomAt(2f / view.Zoom, view.ViewportSize * 0.5f);
            view.CenterOn(point);
            ApplyView();
        }

        void Toggle()
        {
            if (!collapsed && expanded) SetExpanded(false);
            SetCollapsed(!collapsed);
        }

        void SetCollapsed(bool value)
        {
            collapsed = value;
            if (collapsed) gestures.ReleaseAll();
            dock.EnableInClassList("is-collapsed", collapsed);
            collapse.text = collapsed ? "Show" : "Hide";
            collapse.tooltip = collapsed ? "Show minimap" : "Hide minimap";
        }

        void ToggleExpanded()
        {
            if (collapsed) SetCollapsed(false);
            SetExpanded(!expanded);
        }

        void SetExpanded(bool value)
        {
            expanded = value;
            Root.EnableInClassList("is-expanded", expanded);
            Root.BringToFront();
            expand.text = expanded ? "Reduce" : "Expand";
            expand.tooltip = expanded ? "Return to the corner map" : "Open a larger map";
        }

        public void Dispose()
        {
            viewport.RemoveManipulator(gestures);
            viewport.UnregisterCallback<GeometryChangedEvent>(Resize);
            Root.UnregisterCallback<GeometryChangedEvent>(AdaptLayout);
            Root.RemoveFromHierarchy();
        }
    }
}
