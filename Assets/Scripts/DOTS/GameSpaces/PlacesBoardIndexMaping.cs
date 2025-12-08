using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GameSpaces
{
    public partial struct PlacesBoardIndexMapping : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IndexToBoardHashMap>();
            state.RequireForUpdate<BoardIndexComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var map = SystemAPI.GetSingletonRW<IndexToBoardHashMap>().ValueRW.Map;
            new LoadIndexToEntityMapJob { map = map.AsParallelWriter() }.ScheduleParallel();
            state.Enabled = false;
        }

        public partial struct LoadIndexToEntityMapJob : IJobEntity
        {
            public NativeParallelHashMap<int, Entity>.ParallelWriter map;
            public void Execute(Entity entity, in BoardIndexComponent boardIndex)
            {
                map.TryAdd(boardIndex.Value, entity);
            }
        }
    }
}
