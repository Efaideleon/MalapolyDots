using DOTS.UI.Controllers;
using DOTS.UI.Systems;
using Unity.Entities;

public partial struct SpaceActionsPanelUpdaterManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpaceActionsPanelContextComponent>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var spaceActionsContext in
                SystemAPI.Query<
                RefRO<SpaceActionsPanelContextComponent>
                >()
                .WithChangeFilter<SpaceActionsPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.spaceActionsPanelController != null)
                {
                    panelControllers.spaceActionsPanelController.Context = spaceActionsContext.ValueRO.Value;
                }
            }
        }
    }
}
