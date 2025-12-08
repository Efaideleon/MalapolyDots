using DOTS.Characters;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct SpaceDetectorSystem : ISystem
    {
        public ComponentLookup<FinalArrived> finalArrivedLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IndexToBoardHashMap>();
            state.RequireForUpdate<FinalArrived>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<PlayerBoardIndex>();
            state.RequireForUpdate<SpaceLandedOn>();

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<LandedOnSpace>()
            });

            finalArrivedLookup = SystemAPI.GetComponentLookup<FinalArrived>();

            SystemAPI.SetComponent(entity, new LandedOnSpace { entity = Entity.Null });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            finalArrivedLookup.Update(ref state);

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (!finalArrivedLookup.HasComponent(activePlayerEntity))
                return;

            if (finalArrivedLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                var arrived = finalArrivedLookup[activePlayerEntity];
                var spaceLandedOnRW = SystemAPI.GetComponentRW<SpaceLandedOn>(activePlayerEntity);
                var playerBoardIndex = SystemAPI.GetComponent<PlayerBoardIndex>(activePlayerEntity);

                if (!arrived.Value)
                    return;

                if (SystemAPI.GetSingleton<IndexToBoardHashMap>().Map.TryGetValue(playerBoardIndex.Value, out Entity spaceEntity))
                {
                    spaceLandedOnRW.ValueRW.entity = spaceEntity;
                }
            }
        }
    }

    public struct LandedOnSpace : IComponentData
    {
        public Entity entity;
    }
}
