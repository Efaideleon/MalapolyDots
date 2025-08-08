using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraZoomOutPanSystem : ISystem
    {
        const float MaxZoomOut = 20;
        const float MaxZoomIn = 14; // The default zoom level.
        const float ZoomSpeed = 5;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<CameraFieldOfView>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var fieldOfView = SystemAPI.GetSingleton<CameraFieldOfView>();
            var freeCamFlag = SystemAPI.GetSingleton<FreeCameraToggleFlag>();

            // Zoom out if free cam is enabled.
            if (freeCamFlag.Value && fieldOfView.Value <= MaxZoomOut)
            {
                var job = new CameraZoomOutJob()
                {
                    MaxZoomOut = MaxZoomOut,
                    deltaTime = SystemAPI.Time.DeltaTime,
                    ZoomSpeed =  ZoomSpeed
                };
                state.Dependency = job.Schedule(state.Dependency);
            }

            // Zoom in if free cam is disabled.
            if (!freeCamFlag.Value && fieldOfView.Value >= MaxZoomIn) // TODO: Should probably round the max values
            {
                var job = new CameraZoomInJob()
                {
                    MaxZoomIn = MaxZoomIn,
                    deltaTime = SystemAPI.Time.DeltaTime,
                    ZoomSpeed =  ZoomSpeed
                };
                state.Dependency = job.Schedule(state.Dependency);
            }
        }
    }

    public partial struct CameraZoomOutJob : IJobEntity
    {
        public float MaxZoomOut;
        public float deltaTime;
        public float ZoomSpeed;

        public void Execute(ref CameraFieldOfView cameraFieldOfView)
        {
            cameraFieldOfView.Value += deltaTime * ZoomSpeed; // maybe add curve, start fast -> slow down
        }
    }

    public partial struct CameraZoomInJob : IJobEntity
    {

        public float MaxZoomIn;
        public float deltaTime;
        public float ZoomSpeed;

        public void Execute(ref CameraFieldOfView cameraFieldOfView)
        {
            cameraFieldOfView.Value -= deltaTime * ZoomSpeed; // maybe add curve, start fast -> slow down
        }
    }
}
