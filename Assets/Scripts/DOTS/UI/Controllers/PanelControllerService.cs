using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Assets.Scripts.DOTS.UI.Controllers
{
    public class PanelControllerService : IComponentData
    {
        private readonly Dictionary<Type, object> _panelRegistry = new();

        public void Register<T>(T value) where T : class
        {
            _panelRegistry.TryAdd(typeof(T), value);
        }

        public bool TryGet<T>(out T panel) where T : class
        {
            if(_panelRegistry.TryGetValue(typeof(T), out var @output))
            {
                panel = (T)@output;
                return true;
            }
            panel = default;
            return false;
        }
    }
}
