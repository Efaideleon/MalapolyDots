using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.PropertyAnimations
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TreasureAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<ActivePlayer>();
            state.RequireForUpdate<TreasureSpaceTag>();
            state.EntityManager.CreateSingletonBuffer<TreasureAnimationBuffer>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<TreasureRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                foreach (var spaceLandedOn in SystemAPI.Query<RefRO<SpaceLandedOn>>().WithAll<GhostOwnerIsLocal, ActivePlayer>())
                {
                    Entity landedOnEntity = spaceLandedOn.ValueRO.entity;
                    if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity))
                    {
                        // TODO: Now this system only plays the close animaiton, another system sets the open animation.
                        UnityEngine.Debug.Log($"[TreasureAnimationSystem] | Treasure event to close.");
                        SystemAPI.GetComponentRW<AnimationPlayState>(landedOnEntity).ValueRW.Value = PlayState.Playing;
                        SystemAPI.GetComponentRW<CurrentTreasureAnimation>(landedOnEntity).ValueRW.Value = TreasureAnimation.Close;
                        break;
                    }
                }
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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
