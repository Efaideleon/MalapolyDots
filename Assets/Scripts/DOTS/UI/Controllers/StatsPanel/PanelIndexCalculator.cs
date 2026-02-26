using DOTS.UI.Panels;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public class PanelIndexCalculator<T> where T : IAnimatablePanel
    {
        public int CurrentIdx => _current;
        private int _current = 0;
        private readonly IPanelContainer<T> _statsPanelContainer;

        public PanelIndexCalculator(IPanelContainer<T> statsPanelContainer)
        {
            _statsPanelContainer = statsPanelContainer;
        }

        public void SetPanelDisplayIndex()
        {
            var numOfPanels = _statsPanelContainer.NumOfPanels;
            if (numOfPanels <= 0)
            {
                return;
            }

            var panels = _statsPanelContainer.Panels;
            for (int i = 0; i < numOfPanels; i++)
            {
                var panel = panels[i];
                panel.DisplayIndex = GetDisplayIndex(i);
            }
        }

        public void AdvancePanels()
        {
            var numOfPanels = _statsPanelContainer.NumOfPanels;
            if (numOfPanels <= 0)
            {
                return;
            }
            _current = (_current + 1) % numOfPanels;
            SetPanelDisplayIndex();
        }

        private int GetDisplayIndex(int index)
        {
            var numOfPanels = _statsPanelContainer.NumOfPanels;
            return (index + numOfPanels - _current) % numOfPanels;
        }
    }
}
