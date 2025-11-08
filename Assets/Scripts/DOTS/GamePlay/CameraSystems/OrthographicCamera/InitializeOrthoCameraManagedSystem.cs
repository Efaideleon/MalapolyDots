using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    /// <summary>
    /// This systems sets the initial position of the orthographic camera relative to the current player.
    /// </summary>
    public partial struct InitializeOrthoCameraManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthoCamOffset>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<CameraFieldOfView>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            // TODO: Initialize the camera with respect to the pivot.
            var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
            if (!SystemAPI.HasComponent<LocalTransform>(currentPlayer.entity))
                return;

            //var player = SystemAPI.GetComponent<LocalTransform>(currentPlayer.entity);
            var player = new float3(0,0,0);

            var camConfig = SystemAPI.GetSingleton<OrthoCamOffset>();
            var newCamPosition =  player + camConfig.Offset;

            float3 forward = math.normalize(player - newCamPosition);
            var newCamRotation = quaternion.LookRotationSafe(forward, math.up());

            var camFieldOfView = SystemAPI.GetSingleton<CameraFieldOfView>();
             
            Camera.main.transform.SetLocalPositionAndRotation(newCamPosition, newCamRotation);
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = camFieldOfView.Value;
        }
    }
}
