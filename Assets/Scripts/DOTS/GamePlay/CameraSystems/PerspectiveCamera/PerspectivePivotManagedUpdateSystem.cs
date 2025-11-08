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
            state.RequireForUpdate<PivotTransform>();
            state.RequireForUpdate<PerspectiveCameraPivot>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var perspectivePivot = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraPivot>();
            if (perspectivePivot.Instance == null) return;

            var pivot = SystemAPI.GetSingleton<PivotTransform>();
            perspectivePivot.Instance.transform.SetPositionAndRotation(pivot.Position, pivot.Rotation);
        }
    }
}
