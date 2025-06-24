using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    public struct FreeCameraToggleFlag : IComponentData
    {
        public bool Value;
    }

    public partial struct FreeCameraToggleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<FreeCameraToggleFlag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var freeCamState in SystemAPI.Query<RefRW<FreeCameraToggleFlag>>().WithChangeFilter<FreeCameraToggleFlag>())
            {
                UnityEngine.Debug.Log($"freeCamState: {freeCamState.ValueRW.Value}");
            }
        }
    }
}
