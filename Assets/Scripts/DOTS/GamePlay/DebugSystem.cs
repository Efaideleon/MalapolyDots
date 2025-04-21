using Unity.Entities;

namespace DOTS.GamePlay
{
    public partial struct DebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentPlayerID>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            // {
            //     Debug.Break();
            // }
        }
    }
}
