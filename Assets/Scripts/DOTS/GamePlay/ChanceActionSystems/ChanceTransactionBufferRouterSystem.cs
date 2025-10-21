using Unity.Entities;

namespace DOTS.GamePlay.ChanceActionSystems
{
    public partial struct ChanceTransactionBufferRouterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ChanceBufferEvent>();
            state.RequireForUpdate<ChanceBufferEvent>();
        }

        public void OnUpdate(ref SystemState state)
        { }
    }

    public struct ChanceBufferEvent : IBufferElementData
    { }
}
