using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace DOTS.GamePlay
{
    public struct ClickedPropertyComponent : IComponentData
    {
        public Entity entity;
        public InputActionPhase ClickPhase;
    }

    public struct IsSamePropertyClicked : IComponentData
    {
        public bool Value;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(RayCastSystem))]
    [BurstCompile]
    public partial struct PropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<ClickedPropertyComponent>();
            state.EntityManager.CreateSingleton(new UIButtonDirtyFlag { Value = false }); // might need to move this somewhere else; create system
            state.EntityManager.CreateSingleton(new IsSamePropertyClicked { Value = false });
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<ClickedPropertyComponent>();
            state.RequireForUpdate<RayCastResult>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Doesn't handle double tapping the same spot.
            foreach (var hit in SystemAPI.Query<RefRO<RayCastResult>>().WithChangeFilter<RayCastResult>())
            {
                // Don't process raycast data if the user clicked an ui element.
                var isUIButtonClicked = SystemAPI.GetSingleton<UIButtonDirtyFlag>();
                if (isUIButtonClicked.Value)
                    break;

                var clickedProperty = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                var clickPhase = hit.ValueRO.ClickPhase;
                var propertyEntity = hit.ValueRO.PropertyHit.Entity;

                if (propertyEntity != Entity.Null && !SystemAPI.HasComponent<PropertySpaceTag>(propertyEntity))
                {
                    UnityEngine.Debug.LogWarning($"[PropertyClickSystem] | Entity Hit is not a Property");
                }

                clickedProperty.ValueRW.ClickPhase = clickPhase;

                switch (clickPhase)
                {
                    case InputActionPhase.Started: 
                        if (propertyEntity == Entity.Null)
                            clickedProperty.ValueRW.entity = propertyEntity;
                        else
                        {
                            var isSameEntity = propertyEntity == clickedProperty.ValueRO.entity;
                            clickedProperty.ValueRW.entity = isSameEntity ? Entity.Null : propertyEntity;
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
