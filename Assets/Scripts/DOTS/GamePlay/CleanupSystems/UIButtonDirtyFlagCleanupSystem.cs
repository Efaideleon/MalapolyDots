using Unity.Entities;

namespace DOTS.GamePlay.CleanupSystems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(PropertyClickSystem))]
    [UpdateAfter(typeof(CameraPanningSystem))]
    public partial struct UIButtonDirtyFlagCleanupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        { 
            state.RequireForUpdate<UIButtonDirtyFlag>();
        }

        public void OnUpdate(ref SystemState state)
        { 
            foreach (var flag in SystemAPI.Query<RefRW<UIButtonDirtyFlag>>().WithChangeFilter<UIButtonDirtyFlag>())
            {
                if (flag.ValueRO.Value == true)
                {
                    UnityEngine.Debug.Log("Cleaning up UIButtonDirtyFlag");
                    flag.ValueRW.Value = false;
                }
            }
        }
    }
}
