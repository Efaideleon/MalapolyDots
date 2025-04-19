using Unity.Entities;

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

    public void ShowPanel() => Panel.Show();
    public void HidePanel() => Panel.Hide();

    public void Update()
    {
        Panel.UpdateRollLabel(Context.AmountRolled.ToString());
    }

    public void SetEventBufferQuery(EntityQuery query) => RollAmountQuery = query;

    private void DispatchRollEvent()
    {
        var eventBuffer = RollAmountQuery.GetSingletonBuffer<RollEventBuffer>();
        eventBuffer.Add(new RollEventBuffer{});
    }

    public void Dispose()
    {
        Panel.RollButton.clickable.clicked -= DispatchRollEvent;
        Panel.RollButton.clickable.clicked -= Panel.HideRollButton;
    }
}
