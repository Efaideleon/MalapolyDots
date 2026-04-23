using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DebugSettingsInstantiateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<DebugSettingsRef>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var debugSettingsRef = SystemAPI.GetSingleton<DebugSettingsRef>();
            state.EntityManager.Instantiate(debugSettingsRef.Entity);
            state.Enabled = false;
        }
    }
}
