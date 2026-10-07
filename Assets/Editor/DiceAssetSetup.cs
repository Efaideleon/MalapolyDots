using System;
using System.Linq;
using DOTS.GamePlay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DiceAssetSetup
{
    const string Art = "Assets/Art/Dice/";
    const string Prefab = "Assets/Resources/Dice/DiceRollPresentation.prefab";

    [MenuItem("Tools/Malapoly/Dice/Build assets")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Build dice assets outside Play mode.");
        var ivory = Material("Dice_Ivory", new Color(.95f, .91f, .79f), .32f, 0);
        var ink = Material("Dice_Ink", new Color(.035f, .065f, .105f), .27f, 0);
        var felt = Material("Dice_Felt", new Color(.09f, .29f, .39f), .05f, 0);
        var brass = Material("Dice_Brass", new Color(.73f, .48f, .20f), .48f, .4f);
        ConfigureImport(Art + "Malapoly_Die.fbx", ivory, ink);
        ConfigureImport(Art + "Malapoly_Tray.fbx", felt, brass);

        var scene = EditorSceneManager.NewPreviewScene();
        var root = new GameObject("DiceRollPresentation");
        SceneManager.MoveGameObjectToScene(root, scene);
        try
        {
            var presentation = root.AddComponent<DiceRollPresenter>();
            var visuals = new GameObject("Visuals").transform;
            visuals.SetParent(root.transform, false);
            presentation.Visuals = visuals;
            presentation.ResultOverlay = root.AddComponent<DiceResultOverlay>();
            presentation.FirstDie = Die("First die", visuals);
            presentation.SecondDie = Die("Second die", visuals);
            presentation.Hide();
            PrefabUtility.SaveAsPrefabAsset(root, Prefab);
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        AssetDatabase.SaveAssets();
        CreatePreviewScene();
        Debug.Log("Dice assets ready: " + Prefab);
    }

    static Material Material(string name, Color color, float smoothness, float metallic)
    {
        string path = Art + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Malapoly/Dice/Studio"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = Shader.Find("Malapoly/Dice/Studio");
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void ConfigureImport(string path, params Material[] materials)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.importCameras = false;
        importer.importLights = false;
        importer.importAnimation = false;
        importer.isReadable = true;
        importer.globalScale = 1;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        foreach (var material in materials)
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), material.name), material);
        importer.SaveAndReimport();
    }

    static GameObject InstantiateModel(string name, Transform parent)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Art + name);
        if (model == null) throw new InvalidOperationException("Missing exported model: " + name);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
        instance.transform.localPosition = Vector3.zero;
        return instance;
    }

    static ChosenValueDie Die(string name, Transform parent)
    {
        var wrapper = new GameObject(name);
        wrapper.transform.SetParent(parent, false);
        var model = InstantiateModel("Malapoly_Die.fbx", wrapper.transform);
        var die = wrapper.AddComponent<ChosenValueDie>();
        var transforms = model.GetComponentsInChildren<Transform>();
        die.Faces = Enumerable.Range(1, 6).Select(value => transforms.Single(t => t.name == "Face_" + value)).ToArray();
        var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = name + " contact shadow";
        shadow.transform.SetParent(parent, false);
        shadow.transform.localRotation = Quaternion.Euler(90, 0, 0);
        UnityEngine.Object.DestroyImmediate(shadow.GetComponent<Collider>());
        const string shadowPath = Art + "Dice_ContactShadow.mat";
        var shadowMaterial = AssetDatabase.LoadAssetAtPath<Material>(shadowPath);
        if (shadowMaterial == null)
        {
            shadowMaterial = new Material(Shader.Find("Malapoly/Dice/Contact Shadow"));
            AssetDatabase.CreateAsset(shadowMaterial, shadowPath);
        }
        var renderer = shadow.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = shadowMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        die.ContactShadow = shadow.transform;
        return die;
    }

    static void CreatePreviewScene()
    {
        var previous = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            var camera = new GameObject("Dice preview camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 5, -7);
            camera.transform.LookAt(Vector3.zero);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.055f, .075f, .11f);
            camera.nearClipPlane = .1f;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Preview floor";
            floor.transform.position = new Vector3(0, -.1f, 0);
            floor.transform.localScale = new Vector3(30, .2f, 30);
            floor.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "Dice_Felt.mat");
            var light = new GameObject("Soft key light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.transform.rotation = Quaternion.Euler(40, -30, 0);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var presenter = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, scene)).GetComponent<DiceRollPresenter>();
            presenter.PlayOnStart = true;
            presenter.PreviewFirst = 3;
            presenter.PreviewSecond = 5;
            presenter.FollowCamera = true;
            presenter.PresentationCamera = camera;
            presenter.FloorDieSize = 1.2f;
            presenter.Play(3, 5);
            presenter.SampleSequence(DiceRollValues.FloorRollSeconds);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/DicePreview.unity");
        }
        finally
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    public static string Validate()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
        var root = UnityEngine.Object.Instantiate(prefab);
        root.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            var presenter = root.GetComponent<DiceRollPresenter>();
            presenter.FollowCamera = false;
            for (int a = 1; a <= 6; a++)
            for (int b = 1; b <= 6; b++)
            {
                presenter.Play(a, b, (uint)(a * 6 + b));
                foreach (var die in new[] { presenter.FirstDie, presenter.SecondDie })
                {
                    for (int frame = 0; frame <= 120; frame++)
                    {
                        die.Sample(frame / 120f);
                        foreach (var filter in die.GetComponentsInChildren<MeshFilter>())
                        foreach (var vertex in filter.sharedMesh.vertices)
                            if (presenter.Visuals.InverseTransformPoint(filter.transform.TransformPoint(vertex)).y < -.005f)
                                throw new Exception("Dice clipped below the floor.");
                    }
                    if (Vector3.Dot(die.transform.localRotation * die.FaceNormal(die.Value), Vector3.up) < .99999f)
                        throw new Exception("Wrong face up: " + die.Value);
                    for (int face = 1; face <= 3; face++)
                        if (Vector3.Dot(die.FaceNormal(face), die.FaceNormal(7-face)) > -.999f)
                            throw new Exception("Opposite faces do not sum to seven.");
                }
            }
            return "PASS: all 36 pairs finish face-up correctly; all 8,712 sampled die poses clear the floor; opposites sum to seven.";
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    public static void RenderPreview(string path, float progress = 1)
    {
        var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/DicePreview.unity");
        var previous = RenderTexture.active;
        var texture = new RenderTexture(1100, 850, 24);
        var image = new Texture2D(1100, 850, TextureFormat.RGB24, false);
        try
        {
            var roots = scene.GetRootGameObjects();
            var camera = roots.SelectMany(r => r.GetComponentsInChildren<Camera>()).Single();
            camera.scene = scene;
            var presenter = roots.SelectMany(r => r.GetComponentsInChildren<DiceRollPresenter>()).Single();
            presenter.FollowCamera = true;
            presenter.PresentationCamera = camera;
            presenter.Play(3, 5, 14);
            presenter.SampleSequence(progress * DiceRollValues.AnimationSeconds);
            camera.targetTexture = texture;
            camera.Render();
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, 1100, 850), 0, 0);
            image.Apply();
            System.IO.File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(texture);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
