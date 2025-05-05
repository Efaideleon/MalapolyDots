using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.UI.Mediator.Systems.RollPanelSystems
{
    public struct RollPanelVisibleState : IComponentData
    {
        public bool Value;
    }

    public partial struct RollPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new RollPanelVisibleState { Value = false });
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    if (panelControllers.rollPanelController != null)
                    {
                        switch (gameState.ValueRO.State)
                        {
                            case GameState.Rolling:
                                panelControllers.rollPanelController.ShowPanel();
                                SystemAPI.GetSingletonRW<RollPanelVisibleState>().ValueRW.Value = true;
                                break;
                            case GameState.Landing:
                                panelControllers.rollPanelController.HidePanel();
                                SystemAPI.GetSingletonRW<RollPanelVisibleState>().ValueRW.Value = false;
                                break;
                        }
                    }
                }
            }
        }
    }
}
