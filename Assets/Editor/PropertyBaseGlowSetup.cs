using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class PropertyBaseGlowSetup
{
    const string ShaderPath = "Assets/Materials/property_base_glow.shadergraph";
    const string Folder = "Assets/Materials/BuildingBases";
    const string Command = "/tmp/malapoly-base-glow-command";
    [InitializeOnLoadMethod]
    static void Initialize() => EditorApplication.update += Poll;
    static void Poll()
    {
        if (!File.Exists(Command) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        var command = File.ReadAllText(Command).Trim();
        File.Delete(Command);
        try
        {
            if (command == "apply") { Apply(); Verify(); RenderComparison(); }
            if (command == "verify") { Verify(); RenderComparison(); }
        }
        catch (Exception e)
        {
            File.WriteAllText("/tmp/malapoly-base-glow-error.txt", e.ToString());
            Debug.LogException(e);
        }
    }
    [MenuItem("Tools/Malapoly/Update Base Glow Materials")]
    public static void Apply()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
            throw new InvalidOperationException("The property base glow shader did not compile.");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Materials", "BuildingBases");
        int properties = 0, bases = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Buildings" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var children = asset.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.Contains("_m_base_")).ToArray();
            if (children.Length == 0) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var property = root.GetComponent<PropertySpaceAuthoring>();
                var source = root.GetComponent<MeshRenderer>().sharedMaterial;
                string materialPath = Folder + "/" + root.name + "_BaseGlow.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null) { material = new Material(source); AssetDatabase.CreateAsset(material, materialPath); }
                else EditorUtility.CopySerialized(source, material);
                material.name = root.name + "_BaseGlow";
                material.shader = shader;
                material.enableInstancing = true;
                material.SetColor("_BaseGlowColor", MonopolyCrownSystem.GroupColor(property.Data.Color));
                material.SetVector("_BaseGlowState", Vector4.zero);
                material.SetFloat("_BaseMonopolyStrength", 1.8f);
                material.SetFloat("_BasePurchaseStrength", 1.8f);
                EditorUtility.SetDirty(material);
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.Contains("_m_base_")))
                {
                    renderer.sharedMaterial = material;
                    if (!renderer.TryGetComponent<PropertyBaseGlowAuthoring>(out _)) renderer.gameObject.AddComponent<PropertyBaseGlowAuthoring>();
                    var follower = renderer.GetComponent<PropertyLodRendererAuthoring>();
                    if (follower == null || follower.Property != property)
                        throw new InvalidOperationException("Property follower missing on " + renderer.name);
                    bases++;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                properties++;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText("/tmp/malapoly-base-glow-applied.json", $"{{\"materials\":{properties},\"bases\":{bases}}}");
        Debug.Log($"Base glow materials applied: {properties} materials, {bases} bases.");
    }
    [MenuItem("Tools/Malapoly/Verify Base Glow")]
    public static void Verify()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Base glow shader compile error.");
        int bases = 0, materials = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Buildings" }))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true).Where(r => r.name.Contains("_m_base_")).ToArray();
            if (renderers.Length == 0) continue;
            var source = prefab.GetComponent<MeshRenderer>().sharedMaterial;
            if (source.shader == shader) throw new InvalidOperationException("Building body unexpectedly uses base glow.");
            foreach (var renderer in renderers)
            {
                var material = renderer.sharedMaterial;
                if (material == null || material.shader != shader || !material.HasProperty("_BaseGlowState") ||
                    !renderer.TryGetComponent<PropertyBaseGlowAuthoring>(out _))
                    throw new InvalidOperationException("Base glow not assigned: " + renderer.name);
                for (int p = 0; p < source.shader.GetPropertyCount(); p++)
                {
                    var name = source.shader.GetPropertyName(p);
                    if (!material.HasProperty(name)) throw new InvalidOperationException("Building shader property lost: " + name);
                    switch (source.shader.GetPropertyType(p))
                    {
                        case UnityEngine.Rendering.ShaderPropertyType.Texture:
                            if (source.GetTexture(name) != material.GetTexture(name) || source.GetTextureScale(name) != material.GetTextureScale(name) || source.GetTextureOffset(name) != material.GetTextureOffset(name))
                                throw new InvalidOperationException("Building texture changed: " + name);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Float:
                        case UnityEngine.Rendering.ShaderPropertyType.Range:
                            if (source.GetFloat(name) != material.GetFloat(name)) throw new InvalidOperationException("Building material value changed: " + name);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Color:
                            if (source.GetColor(name) != material.GetColor(name)) throw new InvalidOperationException("Building color changed: " + name);
                            break;
                        case UnityEngine.Rendering.ShaderPropertyType.Vector:
                            if (source.GetVector(name) != material.GetVector(name)) throw new InvalidOperationException("Building vector changed: " + name);
                            break;
                    }
                }
                bases++;
            }
            materials++;
        }
        if (bases != 104 || materials != 22) throw new InvalidOperationException("Unexpected base/material count.");
        VerifyGameplay();
        File.WriteAllText("/tmp/malapoly-base-glow-verified.json", $"{{\"materials\":{materials},\"bases\":{bases},\"textures_preserved\":true,\"gameplay_checks_passed\":true}}");
        Debug.Log("Base glow verification PASS: shader, all 104 bases, original textures and material values, monopoly and purchase state transitions.");
    }
    static void VerifyGameplay()
    {
        var animation = new PropertyBaseGlowAnimation();
        if (animation.Advance(2, 0.1f) != 0f) throw new InvalidOperationException("Joining an existing game incorrectly flashes.");
        if (animation.Advance(3, 0.1f) != 1f) throw new InvalidOperationException("House purchase did not flash.");
        float faded = animation.Advance(3, 1f);
        if (faded <= 0 || faded >= 1) throw new InvalidOperationException("Purchase flash did not fade.");
        if (animation.Advance(2, 0.1f) != 0f) throw new InvalidOperationException("House sale did not cancel flash.");
        animation.Advance(3, 0.1f);
        if (animation.Advance(3, 3f) != 0f) throw new InvalidOperationException("Purchase flash did not expire.");
        using var world = new World("Base glow verification");
        var em = world.EntityManager;
        var property = em.CreateEntity(typeof(MonopolyFlagComponent), typeof(HouseCount));
        var renderer = em.CreateEntity(typeof(PropertyLodSource), typeof(PropertyBaseGlowState), typeof(PropertyBaseGlowAnimation));
        em.SetComponentData(renderer, new PropertyLodSource { Value = property });
        var system = world.GetOrCreateSystemManaged<PropertyBaseGlowSystem>();
        system.Update();
        if (em.GetComponentData<PropertyBaseGlowState>(renderer).Value.x != 0) throw new InvalidOperationException("Unowned set glows.");
        em.SetComponentData(property, new MonopolyFlagComponent { Value = true });
        system.Update();
        if (em.GetComponentData<PropertyBaseGlowState>(renderer).Value.x != 1) throw new InvalidOperationException("Monopoly does not glow.");
        em.SetComponentData(property, new HouseCount { Value = 1 });
        system.Update();
        if (em.GetComponentData<PropertyBaseGlowState>(renderer).Value.y != 1) throw new InvalidOperationException("Replicated purchase does not flash.");
        em.SetComponentData(property, new MonopolyFlagComponent());
        em.SetComponentData(property, new HouseCount());
        system.Update();
        if (math.any(em.GetComponentData<PropertyBaseGlowState>(renderer).Value.xy != float2.zero))
            throw new InvalidOperationException("Lost monopoly or sold house stays lit.");
        em.DestroyEntity(property);
        system.Update();
        if (math.any(em.GetComponentData<PropertyBaseGlowState>(renderer).Value != float4.zero)) throw new InvalidOperationException("Missing property remains lit.");
    }
    [MenuItem("Tools/Malapoly/Render Base Glow Comparison")]
    public static void RenderComparison()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        var target = new RenderTexture(720, 600, 24, RenderTextureFormat.ARGBHalf);
        try
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Buildings/Bld_Bomberos.prefab");
            var root = UnityEngine.Object.Instantiate(prefab);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (var g in root.GetComponentsInChildren<LODGroup>()) g.ForceLOD(0);
            var bounds = root.transform.Find("bomberos_m_base_01").GetComponent<MeshRenderer>().bounds;
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>()) if (r.name.Contains("barrier")) r.enabled = false;
            var light = new GameObject("Preview light", typeof(Light));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(light, scene);
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -35, 0);
            var cameraObject = new GameObject("Base glow preview camera", typeof(Camera));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene; camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.4f;
            camera.transform.position = bounds.center + new Vector3(12, 14, -20);
            camera.transform.LookAt(bounds.center + Vector3.up * 2f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.085f);
            camera.allowHDR = true; camera.targetTexture = target;
            var block = new MaterialPropertyBlock();
            Color[] idle = null;
            int monopolyChanges = 0, purchaseChanges = 0;
            for (int state = 0; state < 3; state++)
            {
                foreach (var r in root.GetComponentsInChildren<MeshRenderer>().Where(r => r.name.Contains("_m_base_")))
                {
                    block.SetVector("_BaseGlowState", new Vector4(state == 0 ? 0 : 1, state == 2 ? 1 : 0, 0, 0.8f));
                    r.SetPropertyBlock(block);
                }
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(720, 600, TextureFormat.RGBAFloat, false, true);
                image.ReadPixels(new Rect(0, 0, 720, 600), 0, 0); image.Apply();
                var colors = image.GetPixels();
                if (state == 0) idle = colors;
                else
                {
                    int changed = colors.Where((color, i) => Mathf.Abs(color.r - idle[i].r) + Mathf.Abs(color.g - idle[i].g) + Mathf.Abs(color.b - idle[i].b) > 0.05f).Count();
                    if (state == 1) monopolyChanges = changed; else purchaseChanges = changed;
                    if (changed < 300) throw new InvalidOperationException("Base glow did not visibly render.");
                }
                var display = new Texture2D(720, 600, TextureFormat.RGB24, false);
                display.SetPixels(colors.Select(color => new Color(color.r, color.g, color.b).gamma).ToArray()); display.Apply();
                File.WriteAllBytes($"/tmp/malapoly-base-glow-{state}.png", display.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(display); UnityEngine.Object.DestroyImmediate(image);
            }
            File.WriteAllText("/tmp/malapoly-base-glow-render.json", $"{{\"monopoly_changed_pixels\":{monopolyChanges},\"purchase_changed_pixels\":{purchaseChanges}}}");
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
