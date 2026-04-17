using Assets.Common;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
namespace Assets.Scripts.TitleScreen.NetworkUI.Authoring
{
    public partial struct GoBackToMainMenuSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuPhaseComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (NetworkRequests.GoBackToMainMenu)
            {
                var gameMenuPhase = SystemAPI.GetSingletonRW<GameMenuPhaseComponent>();
                UnityEngine.Debug.Log($"[GoBackToMainMenuSystem] | test");
                gameMenuPhase.ValueRW.Value = GameMenuPhase.MainMenu;
                NetworkRequests.GoBackToMainMenu = false;
            }
        }
    }
}
