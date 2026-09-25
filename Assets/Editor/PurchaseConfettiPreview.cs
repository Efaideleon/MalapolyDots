using DOTS.GamePlay.CameraSystems;
using UnityEditor;
using UnityEngine;

public static class PurchaseConfettiPreview
{
    [MenuItem("Tools/Malapoly/Preview Purchase Confetti")]
    public static void Preview()
    {
        if (!Application.isPlaying || Camera.main == null)
        {
            Debug.LogWarning("Enter Play mode to preview purchase confetti.");
            return;
        }
        var camera = Camera.main;
        var go = new GameObject("Purchase confetti preview");
        var effect = go.AddComponent<PurchaseConfetti>();
        effect.Play(camera.transform.position + camera.transform.forward * 12f - camera.transform.right * 4f);
        Object.Destroy(go, 4f);
        Debug.Log("Purchase confetti preview: emitted 100 pieces.");
    }
}
