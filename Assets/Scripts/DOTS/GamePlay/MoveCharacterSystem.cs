using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct MoveCharacterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnComponent>();
        state.RequireForUpdate<RollAmountComponent>();
        state.RequireForUpdate<WayPointsTag>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        /*foreach (var (turnComponent, prefabComponent, wayPointIndex, entity) in SystemAPI.Query<RefRO<TurnComponent>, RefRW<PrefabReferenceComponent>, RefRW<WayPointsBufferIndex>>().WithEntityAccess())*/
        /*{*/
        /*    // We are always looping this right now...*/
        /*    if (turnComponent.ValueRO.IsActive)*/
        /*    {*/
        /*        // Get the value that we rolled.*/
        /*        var rollAmount = SystemAPI.GetSingleton<RollAmountComponent>();*/
        /*        UnityEngine.Debug.Log($"roll Amount: {rollAmount.Amount}");*/
        /**/
        /*        foreach (var (_, wayPointsEntity) in SystemAPI.Query<RefRO<WayPointsTag>>().WithEntityAccess())*/
        /*        {*/
        /*            // calulating the new position in the waypoints buffer*/
        /*            var wayPointsBuffer = SystemAPI.GetBuffer<WayPointBufferElement>(wayPointsEntity);*/
        /*            int numOfWayPoints = wayPointsBuffer.Length;*/
        /*            int newWayPointIndex = (rollAmount.Amount + wayPointIndex.ValueRO.Index) % numOfWayPoints;*/
        /*            wayPointIndex.ValueRW.Index = newWayPointIndex;*/
        /**/
        /*            UnityEngine.Debug.Log($"new index: {newWayPointIndex}");*/
        /**/
        /*            // moving the entity prefab*/
        /*            var localTransform = SystemAPI.GetComponent<LocalTransform>(prefabComponent.ValueRW.Value);*/
        /*            UnityEngine.Debug.Log($" Prefab: {prefabComponent.ValueRO.Value}");*/
        /*            localTransform.Position = wayPointsBuffer[newWayPointIndex].WayPoint;*/
        /*            localTransform.Rotation = quaternion.identity;*/
        /*            localTransform.Scale = 1f;*/
        /*            SystemAPI.SetComponent(prefabComponent.ValueRW.Value, localTransform);*/
        /*            var newPosition = SystemAPI.GetComponent<LocalTransform>(prefabComponent.ValueRO.Value).Position;*/
        /*            UnityEngine.Debug.Log($"position: { newPosition } ");*/
        /*        }*/
        /*    }*/
        /*}*/
    }
}
