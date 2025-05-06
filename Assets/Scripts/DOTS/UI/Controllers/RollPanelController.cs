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

    public class RollPanelController
    {
        public RollPanel Panel { get; private set; }
        public EntityQuery RollAmountQuery { get; private set; }
        public RollPanelContext Context {get; set; }

        public RollPanelController(RollPanel rollPanel, RollPanelContext context)
        {
            Panel = rollPanel;
            Context = context;
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            Panel.RollButton.clickable.clicked += DispatchRollEvent;
            Panel.RollButton.clickable.clicked += Panel.HideRollButton;
        }

        public void ShowPanel()
        {
            Panel.Show();
            Context = new RollPanelContext { AmountRolled = Context.AmountRolled, IsVisible = true };
        }

        public void HidePanel() 
        {
            Panel.Hide();
            Context = new RollPanelContext { AmountRolled = Context.AmountRolled, IsVisible = false };
        }

        public void Update()
        {
            Panel.UpdateRollLabel(Context.AmountRolled.ToString());
        }

        public void SetEventBufferQuery(EntityQuery query) => RollAmountQuery = query;

        private void DispatchRollEvent()
        {
            UnityEngine.Debug.Log($"Clicked on roll and sent event");
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
