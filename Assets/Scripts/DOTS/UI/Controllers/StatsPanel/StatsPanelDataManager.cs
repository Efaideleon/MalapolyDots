using System.Collections.Generic;
using DOTS.UI.Controllers;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.DOTS.UI.Controllers.StatsPanel
{
    public class StatsPanelDataManager
    {
        private readonly StatsPanelRegistry _registry;
        private readonly Dictionary<FixedString64Bytes, Sprite> _sprites;

        public StatsPanelDataManager(StatsPanelRegistry registry, Dictionary<FixedString64Bytes, Sprite> sprites)
        {
            _registry = registry;
            _sprites = sprites;
        }

        public void LoadPanelData(StatsPanelContext context)
        {
            if (_registry.TryGetPanel(context.Name, out var panel))
            {
                if (_sprites.TryGetValue(context.Name, out var sprite))
                {
                    panel.SetSprite(sprite);
                    panel.SetName(context.Name);
                    panel.SetMoney(context.Money);
                }
                else
                {
                    throw new KeyNotFoundException($"{context.Name} is not in the _characterSpriteRegistry. Can't load panel data.");
                }
            }
        }
    }
}
