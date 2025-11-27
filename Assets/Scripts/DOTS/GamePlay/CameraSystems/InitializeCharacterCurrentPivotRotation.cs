using DOTS.Characters;
using DOTS.GamePlay.CameraSystems.PerspectiveCamera;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct InitializeCharacterCurrentPivotRotation : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentPivotRotation>();
            state.RequireForUpdate<PerspectiveCameraConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var camConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();
            foreach (var currPivotRotation in SystemAPI.Query<RefRW<CurrentPivotRotation>>())
            {
                currPivotRotation.ValueRW.Value = quaternion.AxisAngle(new float3(0, 1, 0), camConfig.Angle);
            }
        }
    }
}
