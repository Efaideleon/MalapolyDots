using UnityEditor;
using UnityEngine;

public class RenameChildren : MonoBehaviour
{
    [MenuItem("Tools/Rename/Remove _mesh From Children")]
    private static void RemoveBuildingSuffix()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No GameObject selected.");
            return;
        }

        var parent = Selection.activeGameObject.transform;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("_mesh"))
            {
                string newName = child.name.Replace("_mesh", "");
                Debug.Log($"Renaming {child.name} → {newName}");
                child.name = newName;
            }
        }
    }
}
