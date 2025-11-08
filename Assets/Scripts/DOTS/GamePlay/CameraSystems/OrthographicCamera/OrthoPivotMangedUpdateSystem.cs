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
            state.RequireForUpdate<PivotTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var orthoPivot = SystemAPI.ManagedAPI.GetSingleton<OrthoCameraPivotInstance>();
            if (orthoPivot.Instance == null) return;

            var pivot = SystemAPI.GetSingleton<PivotTransform>();

            orthoPivot.Instance.transform.position = pivot.Position;
        }
    }
}
