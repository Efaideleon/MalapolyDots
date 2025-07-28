using Unity.Entities;
using Unity.Jobs;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraZoomOutPanSystem : ISystem
    {
        const float MaxZoomOut = 70;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FreeCameraToggleFlag>();
            state.RequireForUpdate<CameraFieldOfView>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var freeCamFlag in SystemAPI.Query<RefRO<FreeCameraToggleFlag>>().WithChangeFilter<FreeCameraToggleFlag>())
            { 
                ref var fieldOfViewRW = ref SystemAPI.GetSingletonRW<CameraFieldOfView>().ValueRW;
                if (freeCamFlag.ValueRO.Value && fieldOfViewRW.Value != MaxZoomOut)
                {
                    var job = new CameraZoomOutJob()
                    {
                        FieldOfView = fieldOfViewRW,
                        // pass MaxZoomOut
                    };
                }

                // if (!freeCamFlag.ValueRO.Value)
                // {
                //     var job = new CameraZoomInJob()
                //     {
                //     };
                // }
            }
        }
    }

    // Wouldn't using a another System to do this be better than using a job to change the value in a IComponentData?
    public partial struct CameraZoomOutJob : IJob
    {
        // Changing a value by reference in job

        // Would FieldOfView be read and write since it a struct but is passed using `ref`?
        public CameraFieldOfView FieldOfView;

        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    public partial struct CameraZoomInJob : IJob
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
