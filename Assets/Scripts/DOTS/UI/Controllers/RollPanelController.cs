using DOTS.EventBuses;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
    public struct RollPanelContext : IComponentData
    {
        public int AmountRolled;
        public bool IsVisible;
    }

    public enum VisibilityState
    {
        Visible,
        NotVisible,
    }

    public enum RollPanelState
    {
        Ready,
        CountingDown,
        Hidden
    }

    public sealed class RollPanelController
    {
        public RollPanel Panel { get; private set; }
        public EntityQuery RollAmountQuery { get; private set; }
        private VisibilityState _visibilityState;
        public RollPanelState State { get; private set; }

        public RollPanelController(RollPanel rollPanel)
        {
            Panel = rollPanel;
            SubscribeEvents();
            State = RollPanelState.Ready;
        }

        public void SetState(RollPanelState state)
        {
            if (State == state)
                return;

            State = state;
            switch(state)
            {
                case RollPanelState.Ready:
                    ShowPanel();
                    ShowRollButton();
                    break;
                case RollPanelState.CountingDown:
                    ShowPanel();
                    HideRollButton();
                    break;
                case RollPanelState.Hidden:
                    HidePanel();
                    HideRollButton();
                    break;
            }
        }

        public bool IsVisible => _visibilityState == VisibilityState.Visible;
        public bool IsHiding => _visibilityState == VisibilityState.NotVisible;

        public void SubscribeEvents()
        {
            Panel.RollButton.clickable.clicked += DispatchRollEvent;
            Panel.RollButton.clickable.clicked += Panel.HideRollButton;
        }

        private void ShowPanel()
        {
            Panel.Show();
            _visibilityState = VisibilityState.Visible;
        }

        private void HidePanel()
        {
            Panel.Hide();
            _visibilityState = VisibilityState.NotVisible;
        }

        private void ShowRollButton() => Panel.ShowRollButton();
        private void HideRollButton() => Panel.HideRollButton();

        public void Update(int rollAmount)
        {
            Panel.UpdateRollLabel(rollAmount.ToString());
        }

        public void SetEventBufferQuery(EntityQuery query) => RollAmountQuery = query;

        private void DispatchRollEvent()
        {
            UnityEngine.Debug.Log($"[RollPanelController] | Clicked on roll and sent event");
            var eventBuffer = RollAmountQuery.GetSingletonBuffer<RollEventBuffer>();
            eventBuffer.Add(new RollEventBuffer{});
        }

        public void Dispose()
        {
            Panel.RollButton.clickable.clicked -= DispatchRollEvent;
            Panel.RollButton.clickable.clicked -= Panel.HideRollButton;
        }
    }
}
