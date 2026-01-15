using TitleScreen.NetworkUI.Components;
using Unity.Entities;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GameMenuUISystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UIEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var evt in SystemAPI.Query<RefRO<MainMenuHostClickEvent>>())
            {
                SystemAPI.GetSingletonRW<GameMenuPhaseComponent>().ValueRW.Value = GameMenuPhase.HostSetup;
                UnityEngine.Debug.Log($"[GameMenuUISystem] | Clicked on MainMenu Host Button");
            }

            foreach (var evt in SystemAPI.Query<RefRO<MainMenuJoinClickEvent>>())
            {
                SystemAPI.GetSingletonRW<GameMenuPhaseComponent>().ValueRW.Value = GameMenuPhase.JoinSetup;
                UnityEngine.Debug.Log($"[GameMenuUISystem] | Clicked on MainMenu Join Button");
            }

            foreach (var evt in SystemAPI.Query<RefRO<HostSetupHostClickEvent>>())
            {
                //gameMenuPhaseLookup.GetRefRW(gameMenuPhaseEntity).ValueRW.Value = GameMenuPhase.JoinSetup;
                UnityEngine.Debug.Log($"[GameMenuUISystem] | Clicked on HostSetup Host Button");
            }

            foreach (var evt in SystemAPI.Query<RefRO<JoinSetupJoinClickEvent>>())
            {
                //gameMenuPhaseLookup.GetRefRW(gameMenuPhaseEntity).ValueRW.Value = GameMenuPhase.JoinSetup;
                UnityEngine.Debug.Log($"[GameMenuUISystem] | Clicked on JoinSetup Join Button");
            }
        }
    }
}
