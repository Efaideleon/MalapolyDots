using System.Collections.Generic;
using DOTS.UI.Panels;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public static class StatsPanelAnimationConstants
    {
        public static readonly List<TimeValue> OneSecondDuration = new() { new(1, TimeUnit.Second) };
        public static readonly List<TimeValue> NoDuration = new() { new(0, TimeUnit.Second) };
    }

    public class PanelDisplayAnimator<T> where T : IAnimatablePanel
    {
        private readonly StatsPanelLayout<T> _layout;
        private readonly IPanelContainer<T> _container;

        public PanelDisplayAnimator(StatsPanelLayout<T> layout, IPanelContainer<T> container)
        {
            _layout = layout;
            _container = container;
        }

        public void AnimateToPositions()
        {
            var panels = _container.Panels;
            foreach (var panel in panels)
            {
                var position = _layout.DisplayPositions[panel.DisplayIndex];
                UnityEngine.Debug.Log($"[StatsPanelController] | DisplayIndex : {panel.DisplayIndex} postion: top: {position.Top} right: {position.Right}");
                panel.AnimateTranslation(StatsPanelAnimationConstants.OneSecondDuration, position);
            }
        }
    }
}
