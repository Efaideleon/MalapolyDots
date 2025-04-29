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
        public VisualElement BigPanelContainer { get; private set; }
        private readonly SelectionHighlighter<PlayerNameMoneyPanel> _selectionHighlighter;

        public StatsPanelController(VisualElement smallPanelsContainer, VisualElement bigPanelContainer, StatsPanelContext context)
        { 
            SmallPanelsContainer = smallPanelsContainer;
            BigPanelContainer = bigPanelContainer;
            _selectionHighlighter = new(HighActivePanel, DisableHighActivePanel);
            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        private void HighActivePanel(PlayerNameMoneyPanel panel)
        {
            panel.HighlightActivePlayerPanel();
            BigPanelContainer.Add(panel.Root);
        }

        private void DisableHighActivePanel(PlayerNameMoneyPanel panel)
        {
            panel.DisableHighlightActivePlayerPanel();
            SmallPanelsContainer.Add(panel.Root);
        }

        public void RegisterPanel(string character, PlayerNameMoneyPanel panel)
        {
            SmallPanelsContainer.Add(panel.Root);
            StatsPanelRegistry.Add(character, panel);
        }

        public void InitializePanels()
        {
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }

        public void Update()
        {
            UnityEngine.Debug.Log($"Panel to update: {Context.Name}");
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            _selectionHighlighter.Select(panel);
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }
    }
}
