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
        var go = new GameObject("Purchase confetti preview");
        var effect = go.AddComponent<PurchaseConfetti>();
        effect.Play();
        Object.Destroy(go, 6f);
        Debug.Log("Purchase confetti preview: emitted 100 pieces.");
    }
}
