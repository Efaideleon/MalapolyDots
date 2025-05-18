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

    public struct IsSamePropertyClicked : IComponentData
    {
        public bool Value;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct PropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<ClickedPropertyComponent>();
            state.EntityManager.CreateSingleton(new UIButtonDirtyFlag { Value = false }); // might need to move this somewhere else
            state.EntityManager.CreateSingleton(new IsSamePropertyClicked { Value = false });
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<ClickedPropertyComponent>();
        }

        [BurstCompile]
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
                if (isButtonDirty.ValueRO.Value)
                {
                    isButtonDirty.ValueRW.Value = false;
                    break;
                }

                collisionWorld.CastRay(input, out RaycastHit hit);

                var clickedProperty = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                
                switch (clickData.ValueRO.Phase)
                {
                    case InputActionPhase.Started:
                        if (hit.Entity == Entity.Null)
                            clickedProperty.ValueRW.entity = hit.Entity;
                        else
                        {
                            var isSameEntity = hit.Entity == clickedProperty.ValueRO.entity;
                            clickedProperty.ValueRW.entity = isSameEntity ? Entity.Null : hit.Entity;
                            SystemAPI.GetSingletonRW<IsSamePropertyClicked>().ValueRW.Value = isSameEntity;
                        }
                        break;
                    case InputActionPhase.Canceled:
                        break;
                }
            }
        }
    }
}
