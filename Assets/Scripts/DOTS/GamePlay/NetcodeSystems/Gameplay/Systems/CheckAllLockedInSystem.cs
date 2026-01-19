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
        }
        public void OnUpdate(ref SystemState state)
        {
            bool allLockedIn = false;
            foreach (var player in SystemAPI.Query<RefRO<PlayerConnectionData>>().WithChangeFilter<PlayerConnectionData>())
            {
                UnityEngine.Debug.Log($"[CheckAllLockedInSystem] | player character: {player.ValueRO.CharacterSelected} locked in: {player.ValueRO.IsLockedIn}");
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

            // Check if all locked in, then send request to change the scene.
            if (allLockedIn && !SystemAPI.HasSingleton<GameMenuToGameSceneTag>())
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<RequestSceneChange>(entity);
            }
        }
    }

    public struct GameMenuToGameSceneTag : IComponentData
    { }
}
