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
        public VisualElement Container { get; private set; }
        private readonly SelectionHighlighter<PlayerNameMoneyPanel> _selectionHighlighter;

        public StatsPanelController(VisualElement container, StatsPanelContext context)
        { 
            Container = container;
            _selectionHighlighter = new(e => e.HighlightActivePlayerPanel(), e => e.DisableHighlightActivePlayerPanel());
            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        public void RegisterPanel(string character, PlayerNameMoneyPanel panel)
        {
            Container.Add(panel.Root);
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
