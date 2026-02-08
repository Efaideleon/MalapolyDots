using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters;
using DOTS.GamePlay.CameraSystems.PerspectiveCamera;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace DOTS.GamePlay.CameraSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct InitializeCharacterCurrentPivotRotation : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentPivotRotation>();
            state.RequireForUpdate<PerspectiveCameraConfig>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();
            state.RequireForUpdate<PivotTransformTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var camConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();
            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayer == Entity.Null || !SystemAPI.HasComponent<CurrentPivotRotation>(activePlayer))
                return;

            var initialRotation = quaternion.AxisAngle(new float3(0, 1, 0), camConfig.Angle);
            var currPivotRotation = SystemAPI.GetComponentRW<CurrentPivotRotation>(activePlayer);
            currPivotRotation.ValueRW.Value = initialRotation;

            if (SystemAPI.HasSingleton<PivotRotation>())
            {
                SystemAPI.GetSingletonRW<PivotRotation>().ValueRW.Value = initialRotation;
            }

            state.Enabled = false;
        }
    }
}
