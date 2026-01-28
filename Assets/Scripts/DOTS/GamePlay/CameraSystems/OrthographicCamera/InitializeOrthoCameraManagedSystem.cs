using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    /// <summary> Store a reference to the orthographic camera managed object. </summary>
    public class OrthographicCameraObject : IComponentData
    {
        public Camera? camera;
    }

    /// <summary>
    /// This systems sets the initial position of the orthographic camera relative to the current player.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializeOrthoCameraManagedSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthoCamOffset>();
            //state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<CameraFieldOfView>();
        }

        public void OnStartRunning(ref SystemState state) { }
        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
        //     if (SystemAPI.HasSingleton<OrthographicCameraObjectIntializedTag>())
        //         return;
        //     // TODO: Initialize the camera with respect to the pivot.
        //
        //     //var player = SystemAPI.GetComponent<LocalTransform>(currentPlayer.entity);
        //     var player = new float3(0, 0, 0);
        //     UnityEngine.Debug.Log($"[InitializeOrthoCameraManagedSystem] | Initializing OrthographicCamera");
        //
        //     var camConfig = SystemAPI.GetSingleton<OrthoCamOffset>();
        //     var newCamPosition = player + camConfig.Offset;
        //
        //     float3 forward = math.normalize(player - newCamPosition);
        //     var newCamRotation = quaternion.LookRotationSafe(forward, math.up());
        //
        //     var camFieldOfView = SystemAPI.GetSingleton<CameraFieldOfView>();
        //
        //     Camera.main.transform.SetLocalPositionAndRotation(newCamPosition, newCamRotation);
        //     Camera.main.orthographic = true;
        //     Camera.main.orthographicSize = camFieldOfView.Value;
        //
        //     var entity = state.EntityManager.CreateEntity();
        //     state.EntityManager.AddComponentObject(entity, new OrthographicCameraObject { camera = Camera.main });
        //
        //     state.EntityManager.CreateSingleton<OrthographicCameraObjectIntializedTag>();
        }
    }

    public struct OrthographicCameraObjectIntializedTag : IComponentData
    { }
}
