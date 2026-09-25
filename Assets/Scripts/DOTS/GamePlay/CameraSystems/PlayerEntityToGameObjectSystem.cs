using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay.CameraSystems;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PlayerEntityToGameObjectSystem : ISystem
    {
        private Entity trackedPlayer;
        private UnityObjectRef<Transform> trackedTarget;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (!SystemAPI.HasComponent<LocalTransform>(player) ||
                !SystemAPI.HasComponent<SpaceLandedOn>(player) || CameraTargetHolder.Instance == null) return;

            var target = CameraTargetHolder.Instance;
            var transform = SystemAPI.GetComponent<LocalTransform>(player);
            var landingPlace = SystemAPI.GetComponent<SpaceLandedOn>(player);
            bool playerChanged = trackedPlayer != player || trackedTarget.Value != target;
            trackedPlayer = player;
            trackedTarget = target;
            target.position = transform.Position;
            // Keep a stable heading: character turns should not spin the camera rig.

            if (SystemAPI.HasBuffer<Child>(landingPlace.entity))
            {
                DynamicBuffer<Child> children = SystemAPI.GetBuffer<Child>(landingPlace.entity);

                foreach (var childEntity in children)
                {
                    if (SystemAPI.HasComponent<CameraLookAtPointTag>(childEntity.Value))
                    {
                        var cameraLookAtPointTransform = SystemAPI.GetComponent<LocalToWorld>(childEntity.Value);
                        if (TargetPlace.Instance != null)
                            TargetPlace.Instance.transform.SetPositionAndRotation(cameraLookAtPointTransform.Position, cameraLookAtPointTransform.Rotation);
                    }
                }
            }

            // Reset after both shot targets are positioned, so no old-player damping survives.
            if (playerChanged)
                CinemachineCameraManager.Instance?.CutToPlayer();
        }
    }
}
