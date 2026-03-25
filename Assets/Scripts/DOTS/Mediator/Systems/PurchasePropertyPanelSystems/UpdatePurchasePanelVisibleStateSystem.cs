using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.Mediator.Authoring;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct UpdatePurchasePanelVisibleStateSystem : ISystem
    {
        public ComponentLookup<LastPropertyInteracted> selectedPropertyLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PurchasePropertyPanelTag>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<LastPropertyInteracted>();

            selectedPropertyLookup = SystemAPI.GetComponentLookup<LastPropertyInteracted>();
        }

        public void OnUpdate(ref SystemState state)
        {
            selectedPropertyLookup.Update(ref state);

            var lastPropertyEntity = SystemAPI.GetSingletonEntity<LastPropertyInteracted>();
            if (selectedPropertyLookup.DidChange(lastPropertyEntity, state.LastSystemVersion))
            {
                var property = selectedPropertyLookup[lastPropertyEntity];
                if (SystemAPI.HasComponent<PropertySpaceTag>(property.entity))
                {
                    var isOwned = SystemAPI.GetComponent<OwnerComponent>(property.entity).ID;
                    if (isOwned == PropertyConstants.Vacant)
                    {
                        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                        var panel = panelControllers?.purchasePropertyPanelController;
                        //panel?.Show();
                    }
                }
            }
        }
    }
}
