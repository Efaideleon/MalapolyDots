using DOTS.EventBuses;
using Unity.Entities;

namespace DOTS.UI.Utilities.UIButtonEvents
{
    public class SetUIButtonFlagEvent : IButtonEvent
    {
        private EntityQuery _query;

        public SetUIButtonFlagEvent(EntityQuery query)
        {
            _query = query;
        }
        public void DispatchEvent()
        {
            var buffer = _query.GetSingletonBuffer<UIButtonEventBus>();
            buffer.Add(new UIButtonEventBus { });
        }
    }
}
