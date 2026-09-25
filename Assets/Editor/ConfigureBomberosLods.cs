using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ConfigureBomberosLods
{
    private const string FbxPath = "Assets/FBX/meshes/lods/bomberos_m_LODs.fbx";
    private const string PrefabPath = "Assets/Prefabs/Buildings/Bld_Bomberos.prefab";

    [MenuItem("Tools/Inspect Bomberos LODs")]
    public static void Inspect()
    {
        var report = new System.Text.StringBuilder();
        foreach (var path in new[] { PrefabPath, FbxPath })
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            report.AppendLine(path);
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh = filter.sharedMesh;
                report.AppendLine($"{filter.name}: vertices={mesh.vertexCount}, triangles={mesh.triangles.Length / 3}, bounds={mesh.bounds}, localPosition={filter.transform.localPosition}, localRotation={filter.transform.localEulerAngles}, localScale={filter.transform.localScale}, rootMatrix={model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix}");
            }
        }
        System.IO.File.WriteAllText("/private/tmp/bomberos-lod-inspection.txt", report.ToString());
        Debug.Log(report.ToString());
    }

    [MenuItem("Tools/Configure Bomberos LODs")]
    public static void Apply()
    {
        AssetDatabase.Refresh();
        var imported = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (imported == null) throw new Exception("Could not import " + FbxPath);

        var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var original = contents.GetComponent<MeshRenderer>();
            var originalMesh = contents.GetComponent<MeshFilter>().sharedMesh;
            if (original == null) throw new Exception("Bomberos must retain its original LOD0 renderer.");
            var meshes = imported.GetComponentsInChildren<MeshFilter>(true);
            var selected = Enumerable.Range(1, 3).Select(level =>
                meshes.Single(m => m.name == "bomberos_m_LOD" + level).sharedMesh).ToArray();
            foreach (var mesh in selected)
            {
                if (mesh.subMeshCount != original.sharedMaterials.Length ||
                    Vector3.Distance(mesh.bounds.center, originalMesh.bounds.center) > 0.15f ||
                    Vector3.Distance(mesh.bounds.size, originalMesh.bounds.size) > 0.15f)
                    throw new Exception("LOD mesh bounds or material slots do not match the original building: " + mesh.name);
            }

            var existing = contents.transform.Find("Bomberos_LODs");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var lodRoot = new GameObject("Bomberos_LODs");
            lodRoot.transform.SetParent(contents.transform, false);
            var levels = new Renderer[4][];
            levels[0] = new Renderer[] { original };
            for (int i = 0; i < selected.Length; i++)
            {
                var child = new GameObject("Bomberos_LOD" + (i + 1));
                child.transform.SetParent(lodRoot.transform, false);
                child.layer = contents.layer;
                GameObjectUtility.SetStaticEditorFlags(child, GameObjectUtility.GetStaticEditorFlags(contents));
                // Mesh-local geometry already matches LOD0. Do not copy Blender's scene placement.
                child.AddComponent<MeshFilter>().sharedMesh = selected[i];
                var renderer = child.AddComponent<MeshRenderer>();
                EditorUtility.CopySerialized(original, renderer);
                renderer.enabled = true;
                var materialSource = child.AddComponent<DOTS.GameSpaces.PropertyLodRendererAuthoring>();
                materialSource.Property = contents.GetComponent<DOTS.GameSpaces.PropertySpaceAuthoring>();
                levels[i + 1] = new Renderer[] { renderer };
            }

            var group = contents.GetComponent<LODGroup>();
            if (group == null) group = contents.AddComponent<LODGroup>();
            group.SetLODs(new[]
            {
                new LOD(0.60f, levels[0]),
                new LOD(0.30f, levels[1]),
                new LOD(0.12f, levels[2]),
                new LOD(0.00f, levels[3])
            });
            // Keep the board building visible at any zoom; use the same hard switches in DOTS and GameObjects.
            group.fadeMode = LODFadeMode.None;
            group.animateCrossFading = false;
            group.RecalculateBounds();
            PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
            var summary = "Configured Bomberos LOD0–LOD3. Triangles: " +
                string.Join(", ", new[] { originalMesh }.Concat(selected).Select(m => m.triangles.Length / 3));
            System.IO.File.WriteAllText("/private/tmp/bomberos-lod-setup.txt", summary);
            Debug.Log(summary);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
        AssetDatabase.SaveAssets();
    }
}
