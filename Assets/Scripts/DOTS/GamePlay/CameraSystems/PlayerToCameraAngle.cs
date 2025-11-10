// using DOTS.Characters;
// using Unity.Collections;
// using Unity.Entities;
//
// #nullable enable
// namespace DOTS.GamePlay.CameraSystems
// {
//     public partial struct PlayerToCameraAngle : ISystem
//     {
//         public void OnCreate(ref SystemState state)
//         {
//             state.RequireForUpdate<PlayerID>();
//             state.RequireForUpdate<PivotTransform>();
//             state.RequireForUpdate<PlayerToCameraAngleData>();
//         }
//
//         public void OnUpdate(ref SystemState state) 
//         { 
//             state.Enabled = false;
//             var map = SystemAPI.GetSingletonRW<PlayerToCameraAngleData>();
//             map.ValueRW.Map = new(6, Allocator.Persistent);
//             var pivot = SystemAPI.GetSingleton<PivotTransform>();
//
//             foreach (var id in SystemAPI.Query<RefRO<PlayerID>>())
//             {
//                 map.ValueRW.Map.TryAdd(id.ValueRO.Value, pivot.Rotation);
//             }
//         }
//
//         public void OnDestroy(ref SystemState state)
//         {
//             var playerToCameraAngleData = SystemAPI.GetSingletonRW<PlayerToCameraAngleData>();
//             playerToCameraAngleData.ValueRO.Map.Clear();
//             playerToCameraAngleData.ValueRO.Map.Dispose();
//         }
//     }
// }
