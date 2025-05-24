using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabPlacerWithRotation : EditorWindow
{
    public GameObject prefab;
    public TextAsset positionRotationFile;

    [MenuItem("Tools/Prefab Placer With Rotation")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPlacerWithRotation>("Prefab Placer With Rotation");
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        positionRotationFile = (TextAsset)EditorGUILayout.ObjectField("Position + Rotation File", positionRotationFile, typeof(TextAsset), false);

        if (GUILayout.Button("Place Prefabs") && prefab != null && positionRotationFile != null)
        {
            PlacePrefabs();
        }
    }

    void PlacePrefabs()
    {
        string[] lines = positionRotationFile.text.Split('\n');
        GameObject parent = new GameObject("PlacedPrefabs");

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length < 6) continue;

            if (float.TryParse(parts[0], out float px) &&
                float.TryParse(parts[1], out float py) &&
                float.TryParse(parts[2], out float pz) &&
                float.TryParse(parts[3], out float rx) &&
                float.TryParse(parts[4], out float ry) &&
                float.TryParse(parts[5], out float rz))
            {
                Vector3 position = new Vector3(px, py, pz);
                Quaternion rotation = Quaternion.Euler(rx, ry, rz);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.SetParent(parent.transform);
                Undo.RegisterCreatedObjectUndo(instance, "Place Prefab With Rotation");
            }
        }

        Debug.Log("Finished placing prefabs with rotations.");
    }
}
