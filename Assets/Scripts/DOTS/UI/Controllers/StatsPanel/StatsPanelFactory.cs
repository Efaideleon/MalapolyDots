using DOTS.UI.Panels;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public class StatsPanelFactory
    {
        private readonly VisualTreeAsset _panelTree;
        private int count = 0;

        public StatsPanelFactory(VisualTreeAsset panelTree)
        {
            _panelTree = panelTree;
        }

        public PlayerNameMoneyPanel CreatePanel()
        {
            count++;
            VisualElement visualElement = _panelTree.Instantiate();
            PlayerNameMoneyPanel panel = new(visualElement, id: count);
            return panel;
        }
    }
}
