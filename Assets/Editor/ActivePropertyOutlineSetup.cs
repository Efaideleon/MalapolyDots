using System;
using System.Linq;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ActivePropertyOutlineSetup
{
    [MenuItem("Tools/Malapoly/Use Thicker Property Outline")]
    public static void UseThickerOutline()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ActivePropertyOutline.mat");
        material.SetFloat("_OutlineWidth", 4f);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssetIfDirty(material);
        Selection.activeObject = material;
        EditorGUIUtility.PingObject(material);
    }

    [MenuItem("Tools/Malapoly/Preview Active Property Outline")]
    public static void Preview()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        var target = new RenderTexture(640, 480, 24);
        var material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        Texture2D image = null;
        var previous = RenderTexture.active;
        try
        {
            material.SetColor("_BaseColor", new Color(0.15f, 0.45f, 0.6f));
            for (int i = 0; i < 2; i++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cube, scene);
                cube.transform.position = new Vector3(i == 0 ? -1.3f : 1.3f, 0, 0);
                cube.layer = LayerMask.NameToLayer(i == 0 ? "Outline" : "IgnoreDepth");
                cube.GetComponent<Renderer>().sharedMaterial = material;
            }
            var go = new GameObject("Outline preview camera", typeof(Camera));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);
            var camera = go.GetComponent<Camera>();
            camera.scene = scene;
            camera.transform.position = new Vector3(0, 2, -7);
            camera.transform.LookAt(Vector3.zero);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            camera.targetTexture = target;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.Render();
            RenderTexture.active = target;
            image = new Texture2D(640, 480, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 640, 480), 0, 0);
            image.Apply();
            System.IO.File.WriteAllBytes("/tmp/malapoly-outline-preview.png", image.EncodeToPNG());
            Debug.Log("Outline render preview saved to /tmp/malapoly-outline-preview.png (left outlined; right plain).");
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(material);
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
        }
    }

    [MenuItem("Tools/Malapoly/Configure Active Property Outline")]
    public static void Configure()
    {
        var mobile = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/Mobile_Renderer.asset");
        var pc = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
        const string materialPath = "Assets/Materials/ActivePropertyOutline.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Malapoly/Active Property Outline"));
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, materialPath);
        }
        EditorUtility.SetDirty(material);
        foreach (var renderer in new[] { mobile, pc })
        {
            foreach (var oldFeature in renderer.rendererFeatures)
            {
                if (oldFeature != null && oldFeature.GetType().Name == "OutlineEffectRendererPassFeature")
                {
                    oldFeature.SetActive(false);
                    EditorUtility.SetDirty(oldFeature);
                }
            }
            for (int pass = 0; pass < 9; pass++)
            {
                var name = pass == 0 ? "Active Property Mask" :
                    pass == 1 ? "Active Property Outline" : "Active Property Outline " + pass;
                var feature = renderer.rendererFeatures.OfType<RenderObjects>().FirstOrDefault(f => f.name == name);
                if (feature == null)
                {
                    feature = ScriptableObject.CreateInstance<RenderObjects>();
                    feature.name = name;
                    AssetDatabase.AddObjectToAsset(feature, renderer);
                    renderer.rendererFeatures.Add(feature);
                }
                feature.settings.passTag = name;
                feature.settings.Event = UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingTransparents;
                feature.settings.filterSettings.LayerMask = 1 << LayerMask.NameToLayer("Outline");
                feature.settings.filterSettings.RenderQueueType = RenderQueueType.Opaque;
                feature.settings.overrideMaterial = material;
                feature.settings.overrideMaterialPassIndex = pass;
                feature.SetActive(true);
                feature.Create();
                EditorUtility.SetDirty(feature);
            }
            // Outline is a selection layer, so it must still receive normal scene rendering.
            var serialized = new SerializedObject(renderer);
            foreach (var field in new[] { "m_OpaqueLayerMask", "m_PrepassLayerMask" })
            {
                var mask = serialized.FindProperty(field);
                if (mask != null) mask.intValue |= 1 << LayerMask.NameToLayer("Outline");
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            renderer.SetDirty();
            EditorUtility.SetDirty(renderer);
        }
        AssetDatabase.SaveAssets();
        Verify();
        Debug.Log("Active property outline configured using built-in Render Objects on mobile and PC renderers.");
    }

    [MenuItem("Tools/Malapoly/Verify Active Property Outline")]
    public static void Verify()
    {
        using var world = new World("Active property outline verification");
        var em = world.EntityManager;
        var system = world.GetOrCreateSystemManaged<OutlineSelectionSystem>();
        var connection = em.CreateEntity(typeof(NetworkStreamInGame));
        var game = em.CreateEntity(typeof(GameStateComponent));
        var turn = em.CreateEntity(typeof(CurrentActivePlayer));
        Entity Property(int layer)
        {
            var entity = em.CreateEntity(typeof(PropertySpaceTag));
            em.AddSharedComponentManaged(entity, new RenderFilterSettings { Layer = layer });
            return entity;
        }
        var first = Property(7);
        var second = Property(7);
        var lod = em.CreateEntity(typeof(PropertyLodSource));
        em.SetComponentData(lod, new PropertyLodSource { Value = first });
        em.AddSharedComponentManaged(lod, new RenderFilterSettings { Layer = 0 });
        var a = em.CreateEntity(typeof(SpaceLandedOn), typeof(ClickedPropertyComponent));
        var b = em.CreateEntity(typeof(SpaceLandedOn), typeof(ClickedPropertyComponent));
        em.SetComponentData(a, new SpaceLandedOn { entity = first });
        em.SetComponentData(b, new SpaceLandedOn { entity = second });
        em.SetComponentData(turn, new CurrentActivePlayer { Entity = a });
        em.SetComponentData(game, new GameStateComponent { State = GameState.Landing });
        var outline = LayerMask.NameToLayer("Outline");
        void Check(Entity entity, int layer, string message)
        {
            if (em.GetSharedComponentManaged<RenderFilterSettings>(entity).Layer != layer)
                throw new Exception("Outline verification failed: " + message);
        }
        system.Update();
        Check(first, outline, "active player property");
        Check(lod, outline, "LOD renderer");
        Check(second, 7, "waiting player property");
        em.SetComponentData(b, new ClickedPropertyComponent { entity = second });
        system.Update();
        Check(first, outline, "waiting player clicks cannot steal highlight");
        Check(second, 7, "clicked property stays unoutlined");
        em.SetComponentData(turn, new CurrentActivePlayer { Entity = b });
        em.SetComponentData(game, new GameStateComponent { State = GameState.Rolling });
        system.Update();
        Check(first, 7, "previous player restored");
        Check(lod, 0, "original LOD layer restored");
        Check(second, outline, "new turn property");
        em.SetComponentData(game, new GameStateComponent { State = GameState.Walking });
        system.Update();
        Check(second, 7, "clear while walking");
        em.SetComponentData(game, new GameStateComponent { State = GameState.Landing });
        em.SetComponentData(b, new SpaceLandedOn { entity = em.CreateEntity() });
        system.Update();
        Check(second, 7, "non-property landing");
        em.SetComponentData(b, new SpaceLandedOn { entity = second });
        system.Update();
        Check(second, outline, "landing again");
        em.DestroyEntity(connection);
        system.Update();
        Check(second, 7, "disconnect clears highlight");
        Debug.Log("Active property outline verification passed: turn switching, LODs, local clicks, walking, non-property landings and disconnect.");
    }
}
