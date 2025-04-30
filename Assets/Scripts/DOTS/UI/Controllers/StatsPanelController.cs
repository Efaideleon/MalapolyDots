using System.Collections.Generic;
using DOTS.UI.Panels;
using Unity.Collections;
using UnityEngine.UIElements;

namespace DOTS.UI.Controllers
{
    public struct StatsPanelContext
    {
        public FixedString64Bytes Name;
        public FixedString64Bytes Money;
    }

    public class StatsPanelController
    {
        public Dictionary<string, PlayerNameMoneyPanel> StatsPanelRegistry;
        public StatsPanelContext Context { get; set; }
        public VisualElement SmallPanelsContainer { get; private set; }
        private readonly StatsPanelsPositionsCalculator _statsPanelsPositionsCalculator;
        private readonly SelectionHighlighter<PlayerNameMoneyPanel> _selectionHighlighter;

        public StatsPanelController(VisualElement smallPanelsContainer, StatsPanelContext context)
        {
            SmallPanelsContainer = smallPanelsContainer;
            SmallPanelsContainer.style.visibility = Visibility.Hidden;
            _selectionHighlighter = new(HighlightActivePanel, DisableHighlightActivePanel);
            _statsPanelsPositionsCalculator = new(SmallPanelsContainer);
            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        private void HighlightActivePanel(PlayerNameMoneyPanel panel)
        {
            panel.HighlightActivePlayerPanel();
        }

        private void DisableHighlightActivePanel(PlayerNameMoneyPanel panel)
        {
            panel.DisableHighlightActivePlayerPanel();
            var count = SmallPanelsContainer.childCount;
            SmallPanelsContainer.hierarchy.Insert(count - 1, panel.Root);
        }

        public void RegisterPanel(string character, PlayerNameMoneyPanel panel)
        {
            SmallPanelsContainer.Add(panel.Root);
            StatsPanelRegistry.Add(character, panel);
        }

        public void InitializePanels()
        {
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            _statsPanelsPositionsCalculator.CalculatePanelPosition(panel.Root);
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }

        public void SetPanelPositions()
        {
            int idx = 0;
            foreach (var kvp in StatsPanelRegistry)
            {
                var panel = kvp.Value;
                panel.Root.style.right = _statsPanelsPositionsCalculator.GetPanelPosition(idx);
                idx++;
            }
            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        // TODO: this is hard to follow, the user needs to know that the context comes first and then select the panel
        // based on the context; it would be easier if the user just selected the panel based on something at the same time.
        public void SelectPanel()
        {
            UnityEngine.Debug.Log($"Selecting panel: {Context.Name}");
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            _selectionHighlighter.Select(panel);
        }

        public void Update()
        {
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            UnityEngine.Debug.Log($"Updating panel: {Context.Name}");
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }
    }
}
