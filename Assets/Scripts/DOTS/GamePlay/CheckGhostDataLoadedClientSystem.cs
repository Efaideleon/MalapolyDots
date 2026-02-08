using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CheckGhostDataLoadedClientSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GhostDataLoadedTag>())
                return;

            var currentActivePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();

            if (currentActivePlayer.Entity != Entity.Null)
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<GhostDataLoadedTag>(entity);
            }
        }
    }

    public struct GhostDataLoadedTag : IComponentData
    { }
}
