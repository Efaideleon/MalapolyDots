using DOTS.Characters;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct RollAmountCountDown : IComponentData
    {
        public int Value;
    }

    public partial struct RollAmountCountDownSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountCountDown>();
            state.EntityManager.CreateSingleton(new RollAmountCountDown { Value = default });
        }

        public void OnUpdate(ref SystemState state)
        { }
    }
}
