using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    public partial struct CheckAllLockedInSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerConnectionData>();
            state.EntityManager.CreateSingleton<AllLockedIn>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var player in SystemAPI.Query<RefRO<PlayerConnectionData>>().WithChangeFilter<PlayerConnectionData>())
            {
                if (player.ValueRO.IsLockedIn)
                {
                    SystemAPI.SetSingleton(new AllLockedIn { Value = true });
                }

                // If at least one is not locked in the set all locked in to false.
                if (!player.ValueRO.IsLockedIn)
                {
                    SystemAPI.SetSingleton(new AllLockedIn { Value = false });
                }
            }
        }
    }

    public struct AllLockedIn : IComponentData
    {
        public bool Value;
    }
}
