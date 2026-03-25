using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.GamePlay.PropertyAnimations
{
    public partial struct TreasureAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LandedOnSpace>();
            state.EntityManager.CreateSingletonBuffer<TreasureAnimationBuffer>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<TreasureAnimationBuffer>>().WithChangeFilter<TreasureAnimationBuffer>())
            {
                var currentSpace = SystemAPI.GetSingleton<LandedOnSpace>();
                if (SystemAPI.HasComponent<TreasureSpaceTag>(currentSpace.entity))
                {
                    foreach (var e in buffer)
                    {
                        switch(e.AnimationType)
                        {
                            case TreasureAnimationType.Open:
                                break;
                            case TreasureAnimationType.Close:
                                UnityEngine.Debug.Log($"[TreasureAnimationSystem] | Treasure event to close.");
                                SystemAPI.GetComponentRW<AnimationPlayState>(currentSpace.entity).ValueRW.Value = PlayState.Playing;
                                SystemAPI.GetComponentRW<CurrentTreasureAnimation>(currentSpace.entity).ValueRW.Value = TreasureAnimation.Close;
                                break;
                        }
                    }
                    buffer.Clear();
                }
            }
        }
    }

    public struct TreasureAnimationBuffer : IBufferElementData
    {
        public TreasureAnimationType AnimationType;
    }
    
    public enum TreasureAnimationType
    {
        Open,
        Close
    }
}
