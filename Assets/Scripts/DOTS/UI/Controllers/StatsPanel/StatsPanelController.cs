using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.Controllers.StatsPanel;
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
        public IReadOnlyDictionary<FixedString64Bytes, PlayerNameMoneyPanel> StatsPanelRegistry => _panelRegister.StatsPanelsLookup;
        private readonly PanelDisplayManager _panelDisplayManager;
        private readonly StatsPanelRegistry _panelRegister;
        private readonly StatsPanelFactory _panelFactory;
        private readonly StatsPanelContainerManager<PlayerNameMoneyPanel> _containerManager;
        private readonly StatsPanelDataManager _dataManager;

        public StatsPanelController(VisualElement smallPanelsContainer, Dictionary<FixedString64Bytes, Sprite> characterSpritesRegistry, VisualTreeAsset panelTree)
        {
            _panelFactory = new(panelTree);
            _containerManager = new(smallPanelsContainer);
            _panelRegister = new(_panelFactory);
            _dataManager = new(_panelRegister, characterSpritesRegistry);
            _panelDisplayManager = new(_containerManager);
            _containerManager.Hide();
        }

        /// <summary>
        /// Creates a panel and registers to a name from the orderedNames into a dictionary.
        /// Also Adds the panel into a VisualElement contianer.
        /// </summary>
        public void SetupPanels(IReadOnlyList<string> orderedNames)
        {
            _panelRegister.Initialize(orderedNames);
            UnityEngine.Debug.Log($"[StatsPanelController] | _panelRegister.PanelsList.Count {_panelRegister.PanelsList.Count}");
            _containerManager.AddPanels(_panelRegister.PanelsList);
        }

        public void LoadPanelData(StatsPanelContext context)
        {
            UnityEngine.Debug.Log($"[StatsPanelController] | loading panel data : {context.Name}");
            _dataManager.LoadPanelData(context);
        }

        public void SetCurrentPanel(FixedString64Bytes playerName)
        {
            int i = 0;
            while(!IsPanelHighlighted(playerName))
            {
                if (i >= _panelRegister.PanelsList.Count)
                {
                    return;
                }
                _panelDisplayManager.AdvancePanels();
                i++;
            }
        }

        private bool IsPanelHighlighted(FixedString64Bytes name)
        {
            _panelRegister.TryGetPanel(name, out var panel);
            return _panelDisplayManager.IsPanelHighlighted(panel);
        }

        public void Dispose()
        {
            _panelDisplayManager.Dispose();
        }
    }
}
