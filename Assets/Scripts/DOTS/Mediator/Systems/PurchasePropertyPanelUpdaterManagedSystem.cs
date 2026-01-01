using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.Mediator;
using DOTS.Mediator.Authoring;
using Unity.Entities;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct PurchasePropertyPanelUpdaterManagedSystem : ISystem
{
    private ComponentLookup<LastPropertyInteracted> lastPropertyInteractedLookup;
    private ComponentLookup<PropertySpaceTag> propertyLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LastPropertyInteracted>();
        state.RequireForUpdate<SpriteRegistryComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<PriceComponent>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<PurchasePropertyPanelData>();

        lastPropertyInteractedLookup = SystemAPI.GetComponentLookup<LastPropertyInteracted>(true);
        propertyLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        lastPropertyInteractedLookup.Update(ref state);
        propertyLookup.Update(ref state);

        // Changes that trigger the system to update the purchase property panel data.
        var entity = SystemAPI.GetSingletonEntity<LastPropertyInteracted>();
        if (!lastPropertyInteractedLookup.DidChange(entity, state.LastSystemVersion)) return;

        var propertyEntity = lastPropertyInteractedLookup[entity].entity;
        if (!propertyLookup.HasComponent(propertyEntity)) return;

        // Getting the data for the purchase property panel.
        var name = SystemAPI.GetComponent<NameComponent>(propertyEntity).Value;
        var price = SystemAPI.GetComponent<PriceComponent>(propertyEntity).Value;

        var spriteRegistry = SystemAPI.ManagedAPI.GetSingleton<SpriteRegistryComponent>();
        spriteRegistry.Value.TryGetValue(name, out var sprite);

        // Update the data to the scriptable object.
        var panel = SystemAPI.ManagedAPI.GetSingleton<PurchasePropertyPanelData>().Panel;

        panel.NameLabel = name.ToString();
        panel.PriceLabel = price.ToString();
        panel.Image = sprite;
    }
}
