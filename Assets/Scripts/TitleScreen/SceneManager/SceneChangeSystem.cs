using Unity.Entities;

public class SceneChange : IComponentData
{
    public SceneLoadersSystem System;
}

public struct SceneChangeEventBuffer : IBufferElementData
{
    public SceneID SceneID;
}

public struct SceneLoaded: IComponentData
{
    public SceneID ID;
}

public partial struct SceneChangeSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new SceneChange { System = null });
        state.EntityManager.CreateSingleton(new SceneLoaded { ID = SceneID.Default });
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
                SystemAPI.GetSingletonRW<SceneLoaded>().ValueRW.ID = e.SceneID;
                sceneChange.System.LoadSceneBySceneIDEnum(e.SceneID);
            }
            buffer.Clear();
        }
    }

    public void OnStopRunning(ref SystemState state) { }
}

