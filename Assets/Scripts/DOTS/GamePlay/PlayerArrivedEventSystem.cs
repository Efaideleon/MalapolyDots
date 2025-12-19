using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct variableName : ISystem
    {
        public BufferLookup<PlayerArrivedEventBuffer> playerArrivedEventBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<PlayerArrivedEventBuffer>();
            playerArrivedEventBufferLookup = SystemAPI.GetBufferLookup<PlayerArrivedEventBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            playerArrivedEventBufferLookup.Update(ref state);
            var bufferEntity = SystemAPI.GetSingletonEntity<PlayerArrivedEventBuffer>();
            if (playerArrivedEventBufferLookup.DidChange(bufferEntity, state.LastSystemVersion))
            {
                playerArrivedEventBufferLookup[bufferEntity].Clear();
            }
        }
    }

    public struct PlayerArrivedEventBuffer : IBufferElementData
    { }
}
