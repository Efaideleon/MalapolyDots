using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    ///<summary>
    ///Updates the transform for the manage instance of the orthographic pivot.
    ///</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct OrthoPivotManagedUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthoCameraPivotInstanceTag>();
            state.RequireForUpdate<PivotTransformTag>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var orthoPivot = SystemAPI.ManagedAPI.GetSingleton<OrthoCameraPivotInstance>();
            if (orthoPivot.Instance == null) return;

            foreach (var (position, rotation) in SystemAPI.Query<RefRO<PivotPosition>, RefRO<PivotRotation>>().WithAll<PivotTransformTag>())
            {
                // TODO tests just position.
                // Uncheck persistent subscene before running game.
                //orthoPivot.Instance.transform.position = position.ValueRO.Value;
                //UnityEngine.Debug.Log($"[OrthoPivotManagedUpdateSystem] | pivot position = {orthoPivot.Instance.transform.position}");
                //UnityEngine.Debug.DrawLine(position.ValueRO.Value, position.ValueRO.Value + new float3(0, 10, 0), UnityEngine.Color.red);
                orthoPivot.Instance.transform.SetPositionAndRotation(position.ValueRO.Value, rotation.ValueRO.Value);
            }
        }
    }
}
