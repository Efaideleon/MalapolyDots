using DOTS.EventBuses;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine.InputSystem;

namespace DOTS.GamePlay
{
    public struct ClickedPropertyComponent : IComponentData
    {
        public Entity entity;
    }

    //[UpdateAfter(typeof(UIEventBusSystem))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct PropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state) 
        {
            state.EntityManager.CreateSingleton<ClickedPropertyComponent>();
            state.EntityManager.CreateSingleton(new UIButtonDirtyFlag { Value = false }); // might need to move this somewhere else
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<ClickedPropertyComponent>();
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state) 
        {
            PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = world.CollisionWorld;

            // doesn't handle double tapping the same stop
            foreach (var clickData in SystemAPI.Query<RefRO<ClickData>>().WithChangeFilter<ClickData>())
            {
                var clickRayCastData = SystemAPI.GetSingleton<ClickRayCastData>();
                RaycastInput input = new()
                {
                    Start = clickRayCastData.RayOrigin,
                    End = clickRayCastData.RayEnd,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = ~0u,
                        GroupIndex = 0
                    }
                };
                var isButtonDirty = SystemAPI.GetSingletonRW<UIButtonDirtyFlag>();
                if (!isButtonDirty.ValueRO.Value)
                {
                    collisionWorld.CastRay(input, out RaycastHit hit);
                    var clickedProperty = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                    clickedProperty.ValueRW = new ClickedPropertyComponent { entity = hit.Entity };
                }
                else
                {
                    isButtonDirty.ValueRW.Value = false;
                }
            }
        }
    }
}
