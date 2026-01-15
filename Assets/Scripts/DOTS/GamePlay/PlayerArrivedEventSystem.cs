using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct variableName : ISystem
    {
        public BufferLookup<PlayerArrivedAtDestinationEvent> playerArrivedEventBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<PlayerArrivedAtDestinationEvent>();
            playerArrivedEventBufferLookup = SystemAPI.GetBufferLookup<PlayerArrivedAtDestinationEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            playerArrivedEventBufferLookup.Update(ref state);
            var bufferEntity = SystemAPI.GetSingletonEntity<PlayerArrivedAtDestinationEvent>();
            if (playerArrivedEventBufferLookup.DidChange(bufferEntity, state.LastSystemVersion))
            {
                playerArrivedEventBufferLookup[bufferEntity].Clear();
            }
        }
    }

    public struct PlayerArrivedAtDestinationEvent : IBufferElementData
    { }
}
