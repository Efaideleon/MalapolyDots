using Unity.Entities;

/*
 * Legacy code potentialy, as we don't need to rotate the PerspectiveCamera by itself, use pivot.
 * This code could be used to rotate the PerspectiveCamera on its own axis.
 */
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public partial struct PerspectiveCameraManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCameraObject>();
            state.RequireForUpdate<MainCameraTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var camComponent = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>();
            if (camComponent.camera == null) return;

            var cam = camComponent.camera;

            foreach (var transform in SystemAPI.Query<
                    RefRO<MainCameraTransform>
            >().WithChangeFilter<MainCameraTransform>())
            {
                if (cam.enabled)
                {
                    //cam.transform.rotation = transform.ValueRO.Rotation;
                }
            }
        }
    }
}
