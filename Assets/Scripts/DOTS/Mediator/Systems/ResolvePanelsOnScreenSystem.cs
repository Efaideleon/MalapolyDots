using System.Linq;
using DOTS.Mediator;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace Assets.Scripts.DOTS.Mediator.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ResolvePanelsOnScreenSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.EntityManager.CreateSingleton(new UIPanelResolved { IsStatsPanelResolved = false });
        }

        public void OnUpdate(ref SystemState state)
        {
            var uiPanelResolved = SystemAPI.GetSingleton<UIPanelResolved>();
            if (!uiPanelResolved.IsStatsPanelResolved)
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

                if (panelControllers == null)
                    return;

                if (panelControllers?.statsPanelController != null)
                {
                    if (panelControllers?.statsPanelController.SmallPanelsContainer.resolvedStyle.width > 0)
                    {
                        UnityEngine.Debug.Log($"[ResolvePanelsOnScreenSystem] | SmallPanelsContainer width: {panelControllers.statsPanelController.SmallPanelsContainer.resolvedStyle.width}");
                        SystemAPI.GetSingletonRW<UIPanelResolved>().ValueRW.IsStatsPanelResolved = true;
                    }
                }
                return;
            }
        }
    }

    // TODO: Maybe we can have the panels themselves tells us if they are resolved?
    public struct UIPanelResolved : IComponentData
    {
        public bool IsStatsPanelResolved;
    }
}
