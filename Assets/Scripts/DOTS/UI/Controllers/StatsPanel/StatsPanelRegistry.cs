using System.Collections.Generic;
using DOTS.UI.Panels;
using Unity.Collections;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public interface IPanelRegistry
    {
        public bool TryGet(FixedString64Bytes key, out PlayerNameMoneyPanel panel);
    }

    public class StatsPanelRegistry
    {
        public IReadOnlyDictionary<FixedString64Bytes, PlayerNameMoneyPanel> StatsPanelsLookup => _registry;
        private readonly Dictionary<FixedString64Bytes, PlayerNameMoneyPanel> _registry;
        public IReadOnlyList<PlayerNameMoneyPanel> PanelsList => _panelsList;
        private readonly List<PlayerNameMoneyPanel> _panelsList;
        private readonly StatsPanelFactory _panelFactory;

        public StatsPanelRegistry(StatsPanelFactory statsPanelFactory)
        {
            _registry = new();
            _panelsList = new();
            _panelFactory = statsPanelFactory;
        }

        public void Initialize(IReadOnlyList<string> list)
        {
            _registry.Clear();
            _panelsList.Clear();
            foreach (var value in list)
            {
                var panel = _panelFactory.CreatePanel();
                _registry.TryAdd(value, panel);
                _panelsList.Add(panel);
            }
        }

        public bool TryGet(FixedString64Bytes key, out PlayerNameMoneyPanel panel)
        {
            return _registry.TryGetValue(key, out panel);
        }
    }
}
