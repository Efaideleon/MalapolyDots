using System.Collections.Generic;
using Assets.Scripts.DOTS.Mediator;
using DOTS.UI.Controllers;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public sealed class StatsPanelDataManager
    {
        private readonly StatsPanelRegistry _panels;
        private readonly ISpriteRegistry _sprites;

        public StatsPanelDataManager(StatsPanelRegistry registry, ISpriteRegistry sprites)
        {
            _panels = registry;
            _sprites = sprites;
        }

        public void LoadPanelData(StatsPanelContext context)
        {
            if (!_panels.TryGet(context.Name, out var panel))
                throw new KeyNotFoundException($"{context.Name} is not in the _panelRegistry. Can't load panel data.");

            if (!_sprites.TryGet(context.Name, out var sprite))
                throw new KeyNotFoundException($"{context.Name} is not in the _characterSpriteRegistry. Can't load panel data.");

            panel.SetSprite(sprite);
            panel.SetName(context.Name);
            panel.SetMoney(context.Money);
        }
    }
}
