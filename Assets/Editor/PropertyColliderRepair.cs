using DOTS.GameSpaces;
using UnityEditor;
using UnityEngine;

public static class PropertyColliderRepair
{
    [MenuItem("Tools/Malapoly/Fit Property Tap Colliders")]
    public static void Apply()
    {
        int count = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Buildings" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var property = root.GetComponent<PropertySpaceAuthoring>();
                if (property == null) continue;
                var mesh = root.GetComponent<MeshFilter>();
                if (mesh == null || mesh.sharedMesh == null)
                    throw new System.InvalidOperationException("Missing property mesh: " + path);
                Bounds bounds = mesh.sharedMesh.bounds;
                Vector3 size = bounds.size;
                // A half-unit margin on each side makes narrow buildings easier to tap.
                size.x = Mathf.Max(2f, size.x + 1f);
                size.z = Mathf.Max(2f, size.z + 1f);
                size.y = Mathf.Max(1f, size.y + 0.5f);
                var box = root.GetComponent<BoxCollider>();
                if (box == null) box = root.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = size;
                box.enabled = true;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log($"[PropertyColliderRepair] {path}: center={box.center}, size={box.size}");
                count++;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[PropertyColliderRepair] Repaired {count} property tap colliders.");
    }
}
