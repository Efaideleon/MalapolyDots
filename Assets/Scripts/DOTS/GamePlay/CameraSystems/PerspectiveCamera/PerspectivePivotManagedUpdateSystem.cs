using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    /// <summary>
    /// This system using the `PivotTransform` struct (unmanged) to update the pivot instances tranform values (manged).
    /// </summary>
    public partial struct PerspectivePivotManagedUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PivotTransformTag>();
            state.RequireForUpdate<PerspectiveCameraPivot>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var perspectivePivot = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraPivot>();
            if (perspectivePivot.Instance == null) return;

            foreach (var (position, rotation) in SystemAPI.Query<RefRO<PivotPosition>, RefRO<PivotRotation>>())
            {
                perspectivePivot.Instance.transform.SetPositionAndRotation(position.ValueRO.Value, rotation.ValueRO.Value);
            }
        }
    }
}
