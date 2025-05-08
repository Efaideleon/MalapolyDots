using DOTS.Mediator;
using DOTS.UI.Controllers;
using DOTS.UI.Systems;
using Unity.Entities;

public partial struct PurchasePropertyPanelUpdaterManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<SpriteRegistryComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var purchasePropertyPanelContext in
                SystemAPI.Query<
                RefRO<PurchasePropertyPanelContextComponent>
                >()
                .WithChangeFilter<PurchasePropertyPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            var spriteRegistry = SystemAPI.ManagedAPI.GetSingleton<SpriteRegistryComponent>();
            if (panelControllers != null && spriteRegistry.Value != null)
            {
                if (panelControllers.purchasePropertyPanelController != null)
                {
                    // TODO: not consistent with the PurhcaseHousePanel.
                    // Here we assigned the Context to the controller instead of the panel itself
                    var context = purchasePropertyPanelContext.ValueRO.Value;
                    spriteRegistry.Value.TryGetValue(context.Name, out var sprite); 
                    panelControllers.purchasePropertyPanelController.Context = context;
                    panelControllers.purchasePropertyPanelController.ManagedContext.sprite = sprite;
                    panelControllers.purchasePropertyPanelController.Update();
                }
            }
        }

    }
}
