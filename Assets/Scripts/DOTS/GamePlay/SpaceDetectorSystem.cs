using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    // [UpdateBefore(typeof(GameUICanvasSystem))]
    public partial struct SpaceDetectorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BoardIndexComponent>();
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<PlayerWaypointIndex>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<WayPointBufferElement>();

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<LandedOnSpace>()
            });

            SystemAPI.SetComponent(entity, new LandedOnSpace { entity = Entity.Null });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var flag in SystemAPI.Query<RefRO<ArrivedFlag>>().WithChangeFilter<ArrivedFlag>())
            {
                if (!flag.ValueRO.Arrived)
                    break;
                // TODO: delete name Ref here
                foreach (var (name, playerID, playerWaypointIndex) in
                        SystemAPI.Query<RefRO<NameComponent>, RefRO<PlayerID>, RefRO<PlayerWaypointIndex>>())
                {
                    var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                    var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                    UnityEngine.Debug.Log($"[SpaceDetectorSystem] | playerWaypointIndex:  {playerWaypointIndex.ValueRO.Value} ");
                    var wayPointSpace = wayPointsBuffer[playerWaypointIndex.ValueRO.Value];
                    UnityEngine.Debug.Log($"[SpaceDetectorSystem] | space name on: {wayPointSpace.Name} ");
                    UnityEngine.Debug.Log($"[SpaceDetectorSystem] | player id: {playerID.ValueRO.Value} player name: {name.ValueRO.Value} ");
                    UnityEngine.Debug.Log($"[SpaceDetectorSystem] | currentPlayerID: {currentPlayerID.Value} ");

                    foreach (var (spaceName, _, spaceEntity) in SystemAPI.Query<
                            RefRO<NameComponent>, RefRO<BoardIndexComponent>>().WithEntityAccess())
                    {
                        if (playerID.ValueRO.Value == currentPlayerID.Value
                                && wayPointSpace.Name == spaceName.ValueRO.Value)
                        {
                            var spaceLandedEntity = SystemAPI.GetSingletonRW<LandedOnSpace>();
                            spaceLandedEntity.ValueRW.entity = spaceEntity;
                            UnityEngine.Debug.Log($"[SpaceDetectorSystem] | Landed on: {wayPointSpace.Name} ");
                        }
                    }

                    if (playerID.ValueRO.Value == currentPlayerID.Value
                            && wayPointSpace.Name == "None")
                    {
                        var spaceLandedEntity = SystemAPI.GetSingletonRW<LandedOnSpace>();
                        spaceLandedEntity.ValueRW.entity = Entity.Null;
                    }
                }
            }
        }
    }

    public struct LandedOnSpace : IComponentData
    {
        public Entity entity;
    }
}
