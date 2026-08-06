using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ConfigureClientTickRateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<ClientTickRate>())
        {
            var clientTickRate = NetworkTimeSystem.DefaultClientTickRate;
            clientTickRate.InterpolationTimeMS = 100; // floor: always buffer ~100ms
            state.EntityManager.CreateSingleton(clientTickRate);
        }
        state.Enabled = false; // only needs to run once
    }
}
