using DOTS.GamePlay;
using DOTS.GamePlay.CameraSystems;
using UnityEditor;
using UnityEngine;

public static class MoneyFeedbackVerification
{
    [MenuItem("Tools/Malapoly/Preview Money Label")]
    public static void Preview()
    {
        if (!Application.isPlaying || Camera.main == null)
        {
            Debug.LogWarning("Enter Play mode to preview the money label.");
            return;
        }
        var go = new GameObject("Money label verification");
        var overlay = go.AddComponent<MoneyFeedbackOverlay>();
        double start = EditorApplication.timeSinceStartup;
        bool shown = false;
        void Tick()
        {
            if (overlay == null || !Application.isPlaying || EditorApplication.timeSinceStartup - start > 10)
            {
                EditorApplication.update -= Tick;
                if (go != null) Object.Destroy(go);
                return;
            }
            if (!shown) shown = overlay.Show(200, MoneyChangeReason.PassGo);
            if (Camera.main != null)
                overlay.Track(Camera.main.transform.position + Camera.main.transform.forward * 10, 0);
        }
        EditorApplication.update += Tick;
    }
}
