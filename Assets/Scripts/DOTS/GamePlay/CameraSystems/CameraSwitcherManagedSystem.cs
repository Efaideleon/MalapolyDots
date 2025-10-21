using Unity.Entities;

using CamSwitcher = DOTS.GamePlay.CameraSystems.CameraSwitcher;
#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This swtiches uses the CameraSwitcher class to switch between orthorgraphic and perspective cameras.
    /// </summary>
    public partial struct CameraSwitcherManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // state.EntityManager.CreateSingleton<CurrentCameraType>();
            // state.RequireForUpdate<CurrentCameraType>();
            // state.RequireForUpdate<CamerasTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // foreach (var camera in SystemAPI.Query<RefRO<CurrentCameraType>>().WithChangeFilter<CurrentCameraType>())
            // {
            //     var entity = SystemAPI.GetSingletonEntity<CamerasTag>();
            //     var perspectiveCam = state.EntityManager.GetComponentObject<PerspectiveCamera>(entity);
            //
            //     if (perspectiveCam.camera != null)
            //     {
            //         switch (camera.ValueRO.camera)
            //         {
            //             case Cameras.Walking:
            //                 perspectiveCam.camera.enabled = true;
            //                 break;
            //             case Cameras.Rolling:
            //                 perspectiveCam.camera.enabled = false;
            //                 break;
            //         }
            //     }
            // }
        }
    }

    public struct CurrentCameraType : IComponentData
    {
        public Cameras camera;
    }
}
