using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct SpawnGeneralGhostStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GeneralGhostStateSpawnReference>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var generalGhostStateRef = SystemAPI.GetSingleton<GeneralGhostStateSpawnReference>();
            state.EntityManager.Instantiate(generalGhostStateRef.Entity);

            state.Enabled = false;
        }
    }
}
