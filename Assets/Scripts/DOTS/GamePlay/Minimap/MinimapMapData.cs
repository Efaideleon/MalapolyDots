using System;
using UnityEngine;

namespace DOTS.GamePlay.Minimap
{
    [CreateAssetMenu(menuName = "Malapoly/Minimap map")]
    public sealed class MinimapMapData : ScriptableObject
    {
        public Texture2D Image;
        public TextAsset CapturedBounds;

        [Serializable]
        sealed class Capture
        {
            public float minX, maxX, minZ, maxZ;
            public string imageRight, imageTop;
        }

        public bool TryGetWorldBounds(out Rect bounds)
        {
            bounds = default;
            if (Image == null || CapturedBounds == null) return false;
            Capture capture;
            try { capture = JsonUtility.FromJson<Capture>(CapturedBounds.text); }
            catch (ArgumentException) { return false; }
            if (capture == null || capture.imageRight != "+X" || capture.imageTop != "+Z" ||
                !float.IsFinite(capture.minX) || !float.IsFinite(capture.maxX) ||
                !float.IsFinite(capture.minZ) || !float.IsFinite(capture.maxZ) ||
                capture.maxX <= capture.minX || capture.maxZ <= capture.minZ) return false;
            bounds = Rect.MinMaxRect(capture.minX, capture.minZ, capture.maxX, capture.maxZ);
            return true;
        }
    }
}
