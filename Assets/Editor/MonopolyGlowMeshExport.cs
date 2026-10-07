using System;
using System.Collections.Generic;
using System.IO;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.GamePlay;
using UnityEditor;
using UnityEngine;

public static class MonopolyGlowMeshExport
{
    [MenuItem("Tools/Malapoly/Verify Monopoly Glow Meshes")]
    public static void Verify()
    {
        using var glow = new MonopolyPropertyGlow();
        int count = 0;
        var ids = new HashSet<int>();
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Buildings" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var property = prefab.GetComponent<PropertySpaceAuthoring>();
            if (property == null || property.Data == null || property.Data.Color < PropertyColor.Brown ||
                property.Data.Color > PropertyColor.Blue) continue;
            if (!ids.Add(property.Data.id)) throw new InvalidOperationException("Duplicate monopoly property ID.");
            for (int level = 0; level <= 4; level++)
            {
                var mesh = glow.GetMesh(property.Data.id, level);
                if (mesh == null || mesh.vertexCount < 24 || mesh.triangles.Length == 0)
                    throw new InvalidOperationException($"Missing glow for {prefab.name}, houses {level}.");
                foreach (var vertex in mesh.vertices)
                    if (!float.IsFinite(vertex.x) || !float.IsFinite(vertex.y) || !float.IsFinite(vertex.z))
                        throw new InvalidOperationException("Non-finite glow mesh vertex.");
                if (glow.GetMesh(property.Data.id, level) != mesh)
                    throw new InvalidOperationException("Glow mesh cache is not reused.");
                count++;
            }
            if (glow.GetMesh(property.Data.id, 5) != glow.GetMesh(property.Data.id, 4))
                throw new InvalidOperationException("Hotel should retain the four-house silhouette.");
        }
        var shader = Resources.Load<Shader>("MonopolyPropertyGlow");
        if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
            throw new InvalidOperationException("Monopoly glow shader failed to compile.");
        Debug.Log($"Monopoly glow verification PASS: {ids.Count} properties, {count} contour meshes, finite geometry, cache reuse, hotel state and shader.");
    }

    [MenuItem("Tools/Malapoly/Render Monopoly Glow Comparison")]
    public static void RenderComparison()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        var target = new RenderTexture(960, 720, 24);
        using var glow = new MonopolyPropertyGlow();
        try
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Bld_Bomberos.prefab");
            var root = UnityEngine.Object.Instantiate(prefab);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var bounds = root.GetComponent<MeshFilter>().sharedMesh.bounds;
            var property = root.GetComponent<PropertySpaceAuthoring>();
            var sun = new GameObject("Preview light", typeof(Light));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(sun, scene);
            sun.GetComponent<Light>().type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(45, -30, 0);
            Color32[] before = null;
            for (int frame = 0; frame < 2; frame++)
            {
                var cameraObject = new GameObject("Glow preview camera", typeof(Camera));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.GetComponent<Camera>();
                camera.scene = scene;
                camera.orthographic = true;
                camera.orthographicSize = bounds.size.x * 0.5f;
                camera.transform.position = bounds.center + new Vector3(24, 25, -32);
                camera.transform.LookAt(bounds.center);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.15f, 0.17f);
                camera.targetTexture = target;
                if (frame == 1)
                    glow.Draw(property.Data.id, 0, root.transform.localToWorldMatrix,
                        MonopolyCrownSystem.GroupColor(property.Data.Color), 0.7f, camera);
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(960, 720, TextureFormat.RGB24, false);
                try
                {
                    image.ReadPixels(new Rect(0, 0, 960, 720), 0, 0);
                    image.Apply();
                    var pixels = image.GetPixels32();
                    if (frame == 0) before = pixels;
                    else
                    {
                        int changed = 0;
                        for (int i = 0; i < pixels.Length; i++)
                            if (Math.Abs(pixels[i].r - before[i].r) + Math.Abs(pixels[i].g - before[i].g) +
                                Math.Abs(pixels[i].b - before[i].b) > 15) changed++;
                        if (changed < 200) throw new InvalidOperationException("Glow did not visibly render.");
                        Debug.Log($"Monopoly glow render PASS: {changed} visibly changed pixels.");
                    }
                    File.WriteAllBytes($"/tmp/malapoly-mesh-glow-{frame}.png", image.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    [Serializable] private class Catalog { public List<Entry> properties = new List<Entry>(); }
    [Serializable] private class Entry
    {
        public int id;
        public string name, source;
        public Vector3[] vertices;
    }

    [MenuItem("Tools/Malapoly/Export Monopoly Mesh Map")]
    public static void Export()
    {
        var catalog = new Catalog();
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Buildings" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var property = prefab.GetComponent<PropertySpaceAuthoring>();
            if (property == null || property.Data == null || property.Data.Color < PropertyColor.Brown ||
                property.Data.Color > PropertyColor.Blue) continue;
            var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            catalog.properties.Add(new Entry { id = property.Data.id, name = prefab.name,
                source = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mesh)), vertices = mesh.vertices });
        }
        var path = Path.Combine(Path.GetTempPath(), "malapoly-monopoly-mesh-map.json");
        File.WriteAllText(path, JsonUtility.ToJson(catalog));
        Debug.Log($"Exported {catalog.properties.Count} monopoly mesh mappings to {path}");
    }
}
