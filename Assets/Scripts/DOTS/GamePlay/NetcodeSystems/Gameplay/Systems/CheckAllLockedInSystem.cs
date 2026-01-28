using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CheckAllLockedInSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerConnectionData>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
        }
        public void OnUpdate(ref SystemState state)
        {
            bool allLockedIn = false;
            foreach (var player in SystemAPI.Query<RefRO<PlayerConnectionData>>().WithChangeFilter<PlayerConnectionData>())
            {
                if (!player.ValueRO.IsLockedIn)
                {
                    allLockedIn = false;
                    return;
                }
                else
                {
                    allLockedIn = true;
                }
            }

            if (allLockedIn && !SystemAPI.HasSingleton<GameMenuToGameSceneTag>())
            {
                SystemAPI.GetSingletonRW<GamePhaseGhostComponent>().ValueRW.GamePhase = GamePhase.Game;
                UnityEngine.Debug.Log($"[CheckAllLockedInSystem] Setting GamePhaseGhost to GamePhase.Game");
            }
        }
    }

    public struct GameMenuToGameSceneTag : IComponentData
    { }
}
