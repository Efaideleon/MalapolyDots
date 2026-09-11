using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay.CameraSystems;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PlayerEntityToGameObjectSystem : ISystem
    {
        private const float PlaceTargetHeight = 3f;
        private const float PlaceFrontOffset = 2f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, landingPlace) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<SpaceLandedOn>>().WithAll<ActivePlayer>())
            {
                CameraTargetHolder.Instance.transform.SetPositionAndRotation(transform.ValueRO.Position, transform.ValueRO.Rotation);

                if (SystemAPI.HasBuffer<Child>(landingPlace.ValueRO.entity))
                {
                    DynamicBuffer<Child> children = SystemAPI.GetBuffer<Child>(landingPlace.ValueRO.entity);

                    foreach (var childEntity in children)
                    {
                        if (SystemAPI.HasComponent<CameraLookAtPointTag>(childEntity.Value))
                        {
                            var cameraLookAtPointTransform = SystemAPI.GetComponent<LocalToWorld>(childEntity.Value);
                            TargetPlace.Instance.transform.SetPositionAndRotation(cameraLookAtPointTransform.Position, cameraLookAtPointTransform.Rotation);
                        }
                    }
                }
            }
        }
    }
}
