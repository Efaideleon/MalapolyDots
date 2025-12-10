using System.Collections.Generic;
using DOTS.DataComponents;

namespace DOTS.Mediator
{
    public class PanelControllersManager
    {
        private readonly Dictionary<SpaceType, IPanelControllerSimple> spaceToControllerMap = new();

        public void Register(SpaceType place, IPanelControllerSimple controller)
        {
            if (!spaceToControllerMap.TryAdd(place, controller))
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[PanelControllersManager] | Place: {place.ToString()}, Controller: {controller.GetType().Name} already exists in the dictionary.");
#endif
            }
        }

        public void Update<T>(SpaceType place, T data)
        {
            spaceToControllerMap.TryGetValue(place, out var controller);

#if UNITY_EDITOR
            if (controller == null)
            {
                UnityEngine.Debug.Log($"[PanelControllersManager] | No PanelController for Place: {place.ToString()}");
                return;
            }
#endif
            if (controller is IPanelControllerNew<T> @new)
            {
                @new.Update(data);
            }
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"[PanelControllersManager] | controller: {controller.GetType().Name} does not implement IPanelControllerNew<T>");
#endif
            }
        }

        public void Show(SpaceType place)
        {
            spaceToControllerMap.TryGetValue(place, out var controller);
            controller?.Show();
#if UNITY_EDITOR
            if (controller == null)
            {
                UnityEngine.Debug.Log($"[PanelControllersManager] | No PanelController for Place: {place.ToString()}");
            }
#endif
        }
    }
}
