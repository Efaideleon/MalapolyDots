using System;
using UnityEngine;

namespace DOTS.GamePlay.Minimap
{
    public sealed class MinimapViewport
    {
        public const float MaxZoom = 8f;
        public Vector2 ImageSize { get; }
        public Vector2 ViewportSize { get; private set; }
        public Vector2 Pan { get; private set; }
        public float Zoom { get; private set; } = 1f;
        public float Scale => Mathf.Min(ViewportSize.x / ImageSize.x, ViewportSize.y / ImageSize.y) * Zoom;
        public Vector2 ScaledSize => ImageSize * Scale;
        public Vector2 Origin => (ViewportSize - ScaledSize) * 0.5f + Pan;

        public MinimapViewport(Vector2 imageSize)
        {
            if (!Finite(imageSize) || imageSize.x <= 0 || imageSize.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(imageSize));
            ImageSize = imageSize;
        }

        public static Vector2 WorldToMap(Vector3 position, Rect bounds) => new(
            (position.x - bounds.xMin) / bounds.width,
            1f - (position.z - bounds.yMin) / bounds.height);

        public Vector2 Project(Vector2 normalizedPosition) => Origin + Vector2.Scale(normalizedPosition, ScaledSize);

        public void Resize(Vector2 size)
        {
            if (!Finite(size) || size.x <= 0 || size.y <= 0) return;
            var center = Scale > 0 ? Divide(ViewportSize * 0.5f - Origin, ScaledSize) : Vector2.one * 0.5f;
            ViewportSize = size;
            CenterOn(center);
        }

        public void ZoomAt(float factor, Vector2 anchor)
        {
            if (!float.IsFinite(factor) || factor <= 0 || !Finite(anchor) || Scale <= 0) return;
            var point = Divide(anchor - Origin, ScaledSize);
            Zoom = Mathf.Clamp(Zoom * factor, 1f, MaxZoom);
            Pan = ClampPan(anchor - Vector2.Scale(point, ScaledSize) - (ViewportSize - ScaledSize) * 0.5f);
        }

        public void PanBy(Vector2 delta)
        {
            if (Finite(delta)) Pan = ClampPan(Pan + delta);
        }

        public void CenterOn(Vector2 normalizedPosition)
        {
            if (Finite(normalizedPosition)) Pan = ClampPan(Vector2.Scale(Vector2.one * 0.5f - normalizedPosition, ScaledSize));
        }

        public void Fit() { Zoom = 1f; Pan = Vector2.zero; }

        Vector2 ClampPan(Vector2 pan)
        {
            var limit = Vector2.Max(Vector2.zero, (ScaledSize - ViewportSize) * 0.5f);
            return new Vector2(Mathf.Clamp(pan.x, -limit.x, limit.x), Mathf.Clamp(pan.y, -limit.y, limit.y));
        }

        static Vector2 Divide(Vector2 a, Vector2 b) => new(a.x / b.x, a.y / b.y);
        static bool Finite(Vector2 value) => float.IsFinite(value.x) && float.IsFinite(value.y);
    }
}
