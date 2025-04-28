using Unity.Entities;

public class SceneChange : IComponentData
{
    public SceneLoadersSystem System;
}

public struct SceneChangeEventBuffer : IBufferElementData
{
    public SceneID SceneID;
}

public struct IsChangingToGameScene: IComponentData
{
    public bool Value;
}

public partial struct SceneChangeSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new SceneChange { System = null });
        state.EntityManager.CreateSingleton(new IsChangingToGameScene { Value = false });
        state.EntityManager.CreateSingletonBuffer<SceneChangeEventBuffer>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        SystemAPI.ManagedAPI.GetSingleton<SceneChange>().System = UnityEngine.Object.FindFirstObjectByType<SceneLoadersSystem>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<SceneChangeEventBuffer>>().WithChangeFilter<SceneChangeEventBuffer>())
        {
            var sceneChange = SystemAPI.ManagedAPI.GetSingleton<SceneChange>();
            if (sceneChange == null)
                break;
            if (sceneChange.System == null)
                break;

            foreach (var e in buffer)
            {
                if (e.SceneID == SceneID.Game)
                {
                    SystemAPI.GetSingletonRW<IsChangingToGameScene>().ValueRW.Value = true;
                    sceneChange.System.LoadSceneBySceneIDEnum(e.SceneID);
                }
            }
            buffer.Clear();
        }
    }

    public void OnStopRunning(ref SystemState state) { }
}

