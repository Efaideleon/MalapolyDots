using System.Collections.Generic;
using System.Linq;
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

        public void TranslatePanelsPosition()
        {
            int idx =  StatsPanelRegistry.Count - 1;
            foreach (var kvp in StatsPanelRegistry)
            {
                var panel = kvp.Value;
                if (idx == StatsPanelRegistry.Count - 1)
                {
                    var position = _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition(panel.Root);
                    panel.Root.style.translate = new Translate(-position.Right, position.Top);
                    UnityEngine.Debug.Log($"Translate {panel.Root.style.translate.value.x.value}");
                }
                else
                {
                    var position = _statsPanelsPositionsCalculator.GetPanelPosition(idx, kvp.Value.Root);
                    panel.Root.style.translate = new Translate(-position.Right, 0);
                    UnityEngine.Debug.Log($"Translate {panel.Root.style.translate.value.x.value}");
                }
                idx--;
            }

            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        public void SetPanelPositions()
        {
            int idx =  StatsPanelRegistry.Count - 1;
            foreach (var kvp in StatsPanelRegistry)
            {
                var panel = kvp.Value;
                if (idx == StatsPanelRegistry.Count - 1)
                {
                    var position = _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition(panel.Root);
                    panel.Root.style.right = position.Right;
                    panel.Root.style.top = position.Top;
                }    
                else 
                {
                    panel.Root.style.right = _statsPanelsPositionsCalculator.GetPanelPosition(idx, kvp.Value.Root).Right;
                    panel.Root.style.top = _statsPanelsPositionsCalculator.GetPanelPosition(idx, kvp.Value.Root).Top;
                }
                idx--;
            }
            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        private void ShiftPanelsPositions()
        {
            var entries = StatsPanelRegistry.ToList();
            if (entries.Count == 0) 
                return;

            var firstEntry = entries.First();
            entries.RemoveAt(0);
            entries.Insert(entries.Count , firstEntry);

            StatsPanelRegistry = entries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // TODO: this is hard to follow, the user needs to know that the context comes first and then select the panel
        // based on the context; it would be easier if the user just selected the panel based on something at the same time.
        public void SelectPanel()
        {
            UnityEngine.Debug.Log($"Selecting panel: {Context.Name}");
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            _selectionHighlighter.Select(panel);
            // SetPanelPositions();
            TranslatePanelsPosition();
            PrintStatsPanelRegistry();
            ShiftPanelsPositions();
            PrintStatsPanelRegistry();
        }

        private void PrintStatsPanelRegistry()
        {
            UnityEngine.Debug.Log($"printing panels dictionary");
            foreach (var kvp in StatsPanelRegistry)
            {
                UnityEngine.Debug.Log($"{kvp.Key}");
            }
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
