using DOTS.UI.Controllers;

namespace DOTS.UI.Utilities.UIButtonEvents
{
    public class HideBackDropEvent : IButtonEvent
    {
        private readonly BackdropController _controller;

        public HideBackDropEvent(BackdropController controller)
        {
            _controller = controller;
        }

        public void DispatchEvent() => _controller.HideBackdrop(); 
    }
}
