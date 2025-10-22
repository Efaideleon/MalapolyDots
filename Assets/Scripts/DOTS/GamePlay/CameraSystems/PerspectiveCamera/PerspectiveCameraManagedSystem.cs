using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public partial struct PerspectiveCameraManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCamera>();
            state.RequireForUpdate<MainCameraTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var camComponent = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCamera>();
            if (camComponent.camera == null) return;

            var cam = camComponent.camera;

            foreach (var transform in SystemAPI.Query<
                    RefRO<MainCameraTransform>
            >().WithChangeFilter<MainCameraTransform>())
            {
                if (cam.enabled)
                {
                    cam.transform.SetPositionAndRotation(transform.ValueRO.Position, transform.ValueRO.Rotation);
                    UnityEngine.Debug.Log("[PerspectiveCameraManagedSystem] | Camera moving");
                }
            }
        }
    }
}
