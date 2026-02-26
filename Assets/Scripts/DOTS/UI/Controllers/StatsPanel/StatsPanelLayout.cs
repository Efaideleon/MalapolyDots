using System.Collections.Concurrent;
using System.Collections.Generic;
using DOTS.UI.Panels;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public struct StatsPanelLayoutPositions
    {
        public int CurrentTop;
        public int OffscreenRight;
        public int StackTop;
    }

    public class StatsPanelLayout<T>
    {
        public IReadOnlyList<StatsPanelPosition> DisplayPositions => _displayPositions;
        private readonly List<StatsPanelPosition> _displayPositions;

        private readonly StatsPanelLayoutPositions _positions;
        private readonly IPanelContainer<T> _panelContainer;

        public StatsPanelLayout(IPanelContainer<T> panelContainer, StatsPanelLayoutPositions positions)
        {
            _positions = positions;
            _panelContainer = panelContainer;
            _displayPositions = new();
        }

        public void Initialize()
        {
            _displayPositions.Clear();
            var highlighPosition = new StatsPanelPosition(_positions.CurrentTop, -(_panelContainer.ContainerWidth - _panelContainer.PanelWidth));
            _displayPositions.Add(highlighPosition);

            for (int i = 0; i < _panelContainer.NumOfPanels - 1; i++)
            {
                var stackPosition = new StatsPanelPosition(_positions.StackTop, -_panelContainer.PanelWidth * i);
                _displayPositions.Add(stackPosition);
            }

            var offscreenPosition = new StatsPanelPosition(_positions.StackTop, _positions.OffscreenRight);
            _displayPositions.Add(offscreenPosition);
        }
    }
}
