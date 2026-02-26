using System;
using System.Collections.Generic;
using DOTS.UI.Panels;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public interface IPanelContainer<T>
    {
        public int NumOfPanels { get; }
        public float PanelWidth { get; }
        public float ContainerWidth { get; }
        public IReadOnlyList<T> Panels { get; }
    }

    public class StatsPanelContainerManager<T> : IPanelContainer<T>, IDisposable where T : Panel
    {
        public IReadOnlyList<T> Panels => _panels;

        ///<summary>
        ///Only resolved after OnContainerGeometryResolved is invoked.
        ///</summary>
        public float PanelWidth => _panels.Count > 0 ? _panels[0].ResolvedWidth : 0f;

        ///<summary>
        ///Only resolved after OnContainerGeometryResolved is invoked.
        ///</summary>
        public float ContainerWidth => _smallPanelsContainer.resolvedStyle.width;

        public int NumOfPanels => _panels.Count;

        ///<summary>
        ///Triggers when the geometry for the panels has been resolved.
        ///</summary>
        public event Action OnContainerGeometryResolved;

        private readonly List<T> _panels;
        private readonly VisualElement _smallPanelsContainer;
        private PanelGeometryResolver<T> _geometryResolver;

        public StatsPanelContainerManager(VisualElement smallPanelsContainer)
        {
            _panels = new();
            _smallPanelsContainer = smallPanelsContainer;
        }

        public void Hide()
        {
            _smallPanelsContainer.style.visibility = Visibility.Hidden;
        }

        public void Show()
        {
            _smallPanelsContainer.style.visibility = Visibility.Visible;
        }

        ///<summary>
        ///Adds a list of panels<T> to a VisualElement container.
        ///</summary>
        public void AddPanels(IReadOnlyList<T> list)
        {
            DisposeResolver();

            _smallPanelsContainer.Clear();
            _panels.Clear();

            _panels.AddRange(list);
            foreach (var panel in list)
            {
                _smallPanelsContainer.Add(panel.Root);
            }

            _geometryResolver = new(list, () => OnContainerGeometryResolved?.Invoke());
        }

        private void DisposeResolver()
        {
            _geometryResolver?.Dispose();
            _geometryResolver = null;
        }

        public void Dispose()
        {
            DisposeResolver();
        }
    }

    internal class PanelGeometryResolver<T> : IDisposable where T : Panel
    {
        private readonly Action _onComplete;

        public IReadOnlyList<T> Panels { get; private set; }
        private int _resolvedPanelCount = 0;
        private int PanelCount => Panels.Count;

        public PanelGeometryResolver(IReadOnlyList<T> panels, Action onComplete)
        {
            Panels = panels;
            _onComplete = onComplete;
            foreach (var panel in Panels)
            {
                panel.Root.RegisterCallback<GeometryChangedEvent>(OnGeometryPanelChanged);
            }
        }

        private void OnGeometryPanelChanged(GeometryChangedEvent evt)
        {
            var element = (VisualElement)evt.target;
            element.UnregisterCallback<GeometryChangedEvent>(OnGeometryPanelChanged);

            _resolvedPanelCount++;
            if (_resolvedPanelCount == PanelCount)
            {
                _onComplete?.Invoke();
            }
        }

        private void UnRegisterPanelCallBacks()
        {
            foreach (var panel in Panels)
            {
                panel.Root.UnregisterCallback<GeometryChangedEvent>(OnGeometryPanelChanged);
            }
        }

        public void Dispose()
        {
            UnRegisterPanelCallBacks();
        }
    }
}

