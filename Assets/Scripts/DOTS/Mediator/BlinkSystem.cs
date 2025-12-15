// using DOTS.Mediator;
// using Unity.Entities;
// using Unity.Rendering;
// public partial struct BlinkSystem : ISystem
// {
//     private float _timer;
//
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<UVScaleOverride>();
//         _timer = 1f;
//     }
//
//     public void OnUpdate(ref SystemState state)
//     {
//         _timer -= SystemAPI.Time.DeltaTime;
//         if (_timer > 0)
//         {
//             return;
//         }
//
//         foreach (var (_, entity) in SystemAPI.Query<RefRO<UVScaleOverride>>().WithEntityAccess())
//         {
//             var mmiState = SystemAPI.IsComponentEnabled<MaterialMeshInfo>(entity);
//             SystemAPI.SetComponentEnabled<MaterialMeshInfo>(entity, !mmiState);
//         }
//
//         _timer = 1;
//     }
// }
