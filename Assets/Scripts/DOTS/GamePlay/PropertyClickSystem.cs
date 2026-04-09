using Assets.Scripts.DOTS.Characters;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    /// <summary>
    /// This systems identifies the property that was tapped.
    /// </summary>
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(RayCastSystem))]
    [BurstCompile]
    public partial struct PropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<TouchStartedInput>();
            state.RequireForUpdate<HitCastResult>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
            foreach (var (touchStarted, clickedProperty, hitCastResult, tappedProperty) in
                    SystemAPI.Query<RefRO<TouchStartedInput>, RefRW<ClickedPropertyComponent>, RefRO<HitCastResult>, RefRW<UITappedPropertyEvent>>().WithAll<Simulate, GhostOwnerIsLocal>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    if (touchStarted.ValueRO.IsTapped.IsSet)
                    {
                        //UnityEngine.Debug.Log($"[PropertyClickSystem] | touchStarted {state.World}");

                        var hitResult = hitCastResult.ValueRO;
                        var objectClickedEntity = hitResult.ObjectHit.Entity;

                        //UnityEngine.Debug.Log($"[PropertyClickSystem] | rayCast: objecthit Entity: {hitResult.ObjectHit.Entity}, Position: {hitResult.ObjectHit.Position} {state.World}");

                        // Check if we tapped on a property.
                        bool isObjectClickedAProperty = objectClickedEntity != Entity.Null && !SystemAPI.HasComponent<PropertySpaceTag>(objectClickedEntity);
                        if (isObjectClickedAProperty)
                        {
                            UnityEngine.Debug.LogWarning($"[PropertyClickSystem] | Entity Hit is not a Property");
                        }

                        // If clicked on nothing, record null and bail out.
                        if (objectClickedEntity == Entity.Null)
                        {
                            clickedProperty.ValueRW.entity = objectClickedEntity;
                            UnityEngine.Debug.Log($"[PropertyClickSystem] | Entity Hit is null");
                            return;
                        }

                        var isSameEntity = objectClickedEntity == clickedProperty.ValueRO.entity;
                        clickedProperty.ValueRW.entity = isSameEntity ? Entity.Null : objectClickedEntity;
                        UnityEngine.Debug.Log($"[PropertyClickSystem] | assigning clickedProperty: {clickedProperty.ValueRO.entity}");

                        UnityEngine.Debug.Log($"[PropertyClickSystem] | setting evetn tick, property was clicked");
                        tappedProperty.ValueRW.entity = clickedProperty.ValueRO.entity;
                        tappedProperty.ValueRW.EventTick = networkTime.ServerTick.SerializedData;
                    }
                }
            }
        }
    }
}
