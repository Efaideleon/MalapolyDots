using DOTS.Characters;
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

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
                    {
                    ComponentType.ReadOnly<LandedOnSpace>()
                    });

            SystemAPI.SetComponent(entity, new LandedOnSpace { entity = Entity.Null });
        }

        //[BurstCompile]
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
                    UnityEngine.Debug.Log($"name: {name.ValueRO.Value} playerID: {playerID.ValueRO.Value} currentPlayerIDf: {currentPlayerID.Value}");
                    foreach (var (boardIndex, spaceEntity) in SystemAPI.Query<RefRO<BoardIndexComponent>>().WithEntityAccess())
                    {
                        if (playerID.ValueRO.Value == currentPlayerID.Value 
                                && playerWaypointIndex.ValueRO.Value == boardIndex.ValueRO.Value)
                        {
                            var spaceLandedEntity = SystemAPI.GetSingletonRW<LandedOnSpace>();
                            spaceLandedEntity.ValueRW.entity = spaceEntity;
                            UnityEngine.Debug.Log("LandedOnSpace!");
                        }
                    }
                }
            }
            // foreach (var (playerID, playerWaypointIndex) in 
            //         SystemAPI.Query<RefRO<PlayerID>, RefRO<PlayerWaypointIndex>>()
            //         .WithChangeFilter<PlayerWaypointIndex>())
            // {
            //     var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
            //     foreach (var (boardIndex, spaceEntity) in SystemAPI.Query<RefRO<BoardIndexComponent>>().WithEntityAccess())
            //     {
            //         if (playerID.ValueRO.Value == currentPlayerID.Value 
            //                 && playerWaypointIndex.ValueRO.Value == boardIndex.ValueRO.Value)
            //         {
            //             var spaceLandedEntity = SystemAPI.GetSingletonRW<LandedOnSpace>();
            //             spaceLandedEntity.ValueRW.entity = spaceEntity;
            //         }
            //     }
            // }
        }
    }

    public struct LandedOnSpace : IComponentData
    {
        public Entity entity;
    }

}
