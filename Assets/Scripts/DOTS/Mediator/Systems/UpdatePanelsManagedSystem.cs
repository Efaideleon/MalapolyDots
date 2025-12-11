using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public partial struct variableName : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
        }
    }

    public struct RequestToUpdateBuffer<T> : IBufferElementData
    {
    }
}
