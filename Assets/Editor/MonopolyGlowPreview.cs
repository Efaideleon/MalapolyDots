using DOTS.GamePlay;
using DOTS.GameSpaces;
using UnityEditor;
using UnityEngine;

public static class MonopolyGlowPreview
{
    private static MonopolyPropertyGlow glow;
    private static PropertySpaceAuthoring[] properties;
    private static double endTime;

    [MenuItem("Tools/Malapoly/Preview Monopoly Glow")]
    public static void Preview()
    {
        Stop();
        var shader = Resources.Load<Shader>("MonopolyPropertyGlow");
        if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
        {
            Debug.LogError("Monopoly glow shader is missing or failed to compile.");
            return;
        }
        properties = Object.FindObjectsByType<PropertySpaceAuthoring>(FindObjectsSortMode.None);
        glow = new MonopolyPropertyGlow();
        endTime = EditorApplication.timeSinceStartup + 30;
        SceneView.duringSceneGui += Draw;
        EditorApplication.update += Tick;
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        Debug.Log($"Monopoly glow preview: {properties.Length} properties for 30 seconds; scene and ownership are unchanged.");
    }

    private static void Draw(SceneView scene)
    {
        if (Event.current.type != EventType.Repaint) return;
        foreach (var property in properties)
        {
            if (property == null || property.Data == null ||
                property.Data.Color < DOTS.DataComponents.PropertyColor.Brown ||
                property.Data.Color > DOTS.DataComponents.PropertyColor.Blue ||
                !property.TryGetComponent<Renderer>(out var renderer))
                continue;
            glow.Draw(property.Data.id, 0, property.transform.localToWorldMatrix, MonopolyCrownSystem.GroupColor(property.Data.Color),
                (float)EditorApplication.timeSinceStartup, scene.camera);
        }
    }

    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup >= endTime || EditorApplication.isPlayingOrWillChangePlaymode)
            Stop();
        SceneView.RepaintAll();
    }

    private static void Stop()
    {
        SceneView.duringSceneGui -= Draw;
        EditorApplication.update -= Tick;
        AssemblyReloadEvents.beforeAssemblyReload -= Stop;
        glow?.Dispose();
        glow = null;
        properties = null;
    }
}
