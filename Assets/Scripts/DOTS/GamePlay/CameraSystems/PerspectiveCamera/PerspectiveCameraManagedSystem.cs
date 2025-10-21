using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public partial struct PerspectiveCameraManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCamera>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var camComponent = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCamera>();
            //camComponent.camera.enabled = true;
        }
    }
}
