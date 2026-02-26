using DOTS.UI.Panels;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public class PanelDisplayManager
    {
        private readonly StatsPanelLayout<PlayerNameMoneyPanel> _layout;
        private readonly PanelIndexCalculator<PlayerNameMoneyPanel> _indexCalc;
        private readonly PanelDisplayAnimator<PlayerNameMoneyPanel> _animator;
        private readonly StatsPanelContainerManager<PlayerNameMoneyPanel> _containerManager;

        public PanelDisplayManager(StatsPanelContainerManager<PlayerNameMoneyPanel> containerManager)
        {
            _containerManager = containerManager;
            _layout = new(_containerManager,
                new StatsPanelLayoutPositions { CurrentTop = 130, OffscreenRight = 350, StackTop = 0 }
            );
            _animator = new(_layout, _containerManager);
            _indexCalc = new(_containerManager);
            _containerManager.OnContainerGeometryResolved += Initialize;
        }

        public bool TryGetCurrentPanel(out PlayerNameMoneyPanel panel)
        {
            if (_indexCalc != null)
            {
                var panels = _containerManager.Panels;
                panel = panels[_indexCalc.CurrentIdx];
                return true;
            }
            panel = null;
            return false;
        }

        public void Initialize()
        {
            var numOfPanels = _containerManager.NumOfPanels;
            UnityEngine.Debug.Log($"[StatsPanelController] | initializing panels numOfPanels: {numOfPanels}");
            if (numOfPanels <= 0)
            {
                return;
            }
            _layout.Initialize();
            _containerManager.Show();
            _indexCalc.SetPanelDisplayIndex();
            _animator.AnimateToPositions();
            if (TryGetCurrentPanel(out var currentPanel))
            {
                currentPanel.HighlightPanel();
            }
            UnityEngine.Debug.Log($"[StatsPanelController] | numOfPanels: {numOfPanels}");
            UnityEngine.Debug.Log($"[StatsPanelController] | initialized PanelDisplayLayout");
            UnityEngine.Debug.Log($"[StatsPanelController] | can't initialized PanelDisplayLayout");
        }

        public void MoveCurrentToOffscreen()
        {
            if (TryGetCurrentPanel(out var CurrentPanel))
            {
                if (_layout.DisplayPositions.Count > 0)
                {
                    CurrentPanel.DisplayIndex = _layout.DisplayPositions.Count - 1;
                    var position = _layout.DisplayPositions[CurrentPanel.DisplayIndex];
                    UnityEngine.Debug.Log($"[StatsPanelController] | Disabling Panel Highlight");
                    CurrentPanel.DisableHighlightPanel();
                    CurrentPanel.AnimateTranslation(StatsPanelAnimationConstants.NoDuration, position);
                }
            }
        }

        public void AdvancePanels()
        {
            MoveCurrentToOffscreen();
            _indexCalc.AdvancePanels();

            if (TryGetCurrentPanel(out var CurrentPanel) && _indexCalc != null)
            {
                CurrentPanel.HighlightPanel();
            }
            _animator.AnimateToPositions();
        }

        public bool IsPanelHighlighted(PlayerNameMoneyPanel panel)
        {
            if (panel != null)
            {
                // if the panel for the active player is not selected then select it and rotate the panels?
                TryGetCurrentPanel(out var CurrentPanel);
                {
                    if (!panel.Equals(CurrentPanel))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            _containerManager.OnContainerGeometryResolved -= Initialize;
            _containerManager.Dispose();
        }
    }
}
