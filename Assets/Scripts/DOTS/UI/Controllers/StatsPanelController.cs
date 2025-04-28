using DOTS.UI.Panels;
using Unity.Collections;

public struct StatsPanelContext
{
    public FixedString64Bytes Name;
    public FixedString64Bytes Money;
}

public class StatsPanelController
{
    public PlayerNameMoneyPanel StatsPanel { get; private set; }
    public StatsPanelContext Context { get; set; }

    public StatsPanelController(PlayerNameMoneyPanel panel, StatsPanelContext context)
    {
        Context = context;
        StatsPanel = panel;
    }

    public void Update()
    {
        StatsPanel.UpdatePlayerNameLabelText(Context.Name.ToString());
        StatsPanel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
    }
}
