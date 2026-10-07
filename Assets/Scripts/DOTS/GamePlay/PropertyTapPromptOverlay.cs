using Assets.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.GamePlay
{
    [DefaultExecutionOrder(10000)]
    public sealed class PropertyTapPromptOverlay : MonoBehaviour
    {
        VisualElement root, prompt;
        Texture2D handCursor;
        Vector3 anchor;
        bool showPrompt, hovered, cursorSet;

        public void Bind(VisualElement foreground)
        {
            if (root?.parent == foreground && root.panel != null) return;
            root?.RemoveFromHierarchy();
            root = Resources.Load<VisualTreeAsset>("UI/PropertyTapPrompt").CloneTree();
            root.AddToClassList("property-tap-layer");
            root.pickingMode = PickingMode.Ignore;
            foreground.Add(root);
            prompt = root.Q("propertyTapPrompt");
            root.Q("propertyTapHand").Add(new PropertyTapHandIcon());
        }

        public void Track(Vector3 worldAnchor, bool visible, bool isHovered)
        {
            anchor = worldAnchor;
            showPrompt = visible;
            hovered = isHovered;
        }

        void LateUpdate()
        {
            SetHandCursor(hovered && !SoloSession.Paused && Application.isFocused &&
                UnityEngine.InputSystem.Mouse.current != null && UnityEngine.Cursor.lockState == CursorLockMode.None);
            var camera = Camera.main;
            bool visible = showPrompt && !SoloSession.Paused && root?.panel != null && camera != null;
            if (visible)
            {
                var viewport = camera.WorldToViewportPoint(anchor);
                visible = viewport.z > camera.nearClipPlane && viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1;
            }
            if (visible)
            {
                var point = root.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, anchor, camera));
                float width = prompt.resolvedStyle.width;
                float height = prompt.resolvedStyle.height;
                if (!float.IsFinite(width) || width <= 0) width = 200;
                if (!float.IsFinite(height) || height <= 0) height = 98;
                float x = point.x - width * 0.5f;
                float y = point.y - height - 12 + Mathf.Sin(Time.unscaledTime * 2.6f) * 4;
                visible = x > 8 && y > 8 && x + width < root.contentRect.width - 8 && y + height < root.contentRect.height - 8;
                if (visible) { prompt.style.left = x; prompt.style.top = y; }
            }
            prompt?.EnableInClassList("is-hidden", !visible);
        }

        void SetHandCursor(bool value)
        {
            if (cursorSet == value) return;
            cursorSet = value;
            if (value && handCursor == null) handCursor = PropertyTapHandIcon.CreateCursor();
            UnityEngine.Cursor.SetCursor(value ? handCursor : null, value ? new Vector2(19, 10) : Vector2.zero, CursorMode.Auto);
        }

        public void Hide()
        {
            showPrompt = hovered = false;
            prompt?.AddToClassList("is-hidden");
            SetHandCursor(false);
        }

        void OnDisable() => Hide();
        void OnDestroy()
        {
            Hide();
            root?.RemoveFromHierarchy();
            if (handCursor != null) Destroy(handCursor);
        }
    }

    sealed class PropertyTapHandIcon : VisualElement
    {
        static readonly Vector2[] Shape = {
            new(16, 38), new(10, 32), new(8, 28), new(9, 25), new(12, 25), new(16, 29),
            new(16, 12), new(17, 9), new(20, 9), new(21, 12), new(21, 22), new(24, 20),
            new(27, 22), new(30, 21), new(33, 23), new(36, 23), new(38, 26), new(38, 34),
            new(35, 42), new(22, 42), new(19, 40)
        };

        public PropertyTapHandIcon()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("property-tap-hand-icon");
            generateVisualContent += Draw;
        }

        void Draw(MeshGenerationContext context)
        {
            if (contentRect.width < 1 || contentRect.height < 1) return;
            var painter = context.painter2D;
            painter.lineWidth = 2;
            painter.lineJoin = LineJoin.Round;
            painter.strokeColor = new Color(0.05f, 0.2f, 0.25f);
            painter.fillColor = Color.white;
            painter.BeginPath();
            painter.MoveTo(Shape[0]);
            for (int i = 1; i < Shape.Length; i++) painter.LineTo(Shape[i]);
            painter.ClosePath(); painter.Fill(); painter.Stroke();
            painter.strokeColor = new Color(0.65f, 0.96f, 1f);
            painter.lineCap = LineCap.Round;
            foreach (float radius in new[] { 5f, 9f })
            {
                painter.BeginPath();
                painter.Arc(new Vector2(19, 10), radius, Angle.Degrees(210), Angle.Degrees(330));
                painter.Stroke();
            }
        }

        public static Texture2D CreateCursor()
        {
            const int size = 48;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    bool inside = false;
                    float edge = float.MaxValue;
                    for (int i = 0, j = Shape.Length - 1; i < Shape.Length; j = i++)
                    {
                        var a = Shape[i]; var b = Shape[j];
                        if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                        var delta = b - a;
                        edge = Mathf.Min(edge, Vector2.Distance(p, a + delta * Mathf.Clamp01(Vector2.Dot(p - a, delta) / delta.sqrMagnitude)));
                    }
                    pixels[(size - 1 - y) * size + x] = edge < 1.25f ? new Color32(14, 42, 48, 255) :
                        inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) {
                name = "Property action hand cursor", hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Point
            };
            texture.SetPixels32(pixels); texture.Apply(false, false);
            return texture;
        }
    }
}
