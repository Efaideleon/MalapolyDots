using System.Collections.Generic;
using System.ComponentModel;
using DOTS.UI.Panels;
using Unity.Collections;
using UnityEngine;
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

        public StatsPanelController(VisualElement container, StatsPanelContext context)
        { 
            Container = container;
            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        public void RegisterPanel(string character, PlayerNameMoneyPanel panel)
        {
            Container.Add(panel.Root);
            StatsPanelRegistry.Add(character, panel);
        }

        public void Update()
        {
            UnityEngine.Debug.Log($"Panel to update: {Context.Name}");
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }
    }
}
