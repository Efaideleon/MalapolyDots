using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    ///<summary>
    ///Updates the transform for the manage instance of the orthographic pivot.
    ///</summary>
    public partial struct OrthoPivotManagedUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthoCameraPivotInstanceTag>();
            state.RequireForUpdate<PivotTransformTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var orthoPivot = SystemAPI.ManagedAPI.GetSingleton<OrthoCameraPivotInstance>();
            if (orthoPivot.Instance == null) return;

            foreach (var (position, rotation) in SystemAPI.Query<RefRO<PivotPosition>, RefRO<PivotRotation>>())
            {
                orthoPivot.Instance.transform.position = position.ValueRO.Value;
            }
        }
    }
}
