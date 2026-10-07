using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.GamePlay.CameraSystems
{
    // A screen-space label follows a world-space head position without accepting input.
    [DefaultExecutionOrder(10000)]
    public class MoneyFeedbackOverlay : MonoBehaviour
    {
        Label label;
        UIDocument document;
        PanelSettings panelSettings;
        GameObject overlayObject;
        Vector3 head;
        float progress;
        bool visible;

        public bool Show(int delta, MoneyChangeReason reason)
        {
            if (document == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "Money feedback panel";
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.match = 0.5f;
                panelSettings.sortingOrder = 1000;
                overlayObject = new GameObject("Money feedback overlay");
                overlayObject.transform.SetParent(transform, false);
                document = overlayObject.AddComponent<UIDocument>();
                document.panelSettings = panelSettings;
            }
            var root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute;
            root.style.left = root.style.right = root.style.top = root.style.bottom = 0;
            if (label == null || label.parent != root)
            {
                label?.RemoveFromHierarchy();
                label = new Label { pickingMode = PickingMode.Ignore, name = "money-feedback" };
                label.style.position = Position.Absolute;
                label.style.width = 240;
                label.style.height = 76;
                var theme = Resources.Load<StyleSheet>("UI/GameTheme");
                if (theme != null && !root.styleSheets.Contains(theme)) root.styleSheets.Add(theme);
                label.AddToClassList("game-money-feedback");
                root.Add(label);
            }
            if (root.panel == null || root.contentRect.width <= 0 || root.contentRect.height <= 0)
                return false;
            string caption = reason switch
            {
                MoneyChangeReason.PassGo => "PASSED GO",
                MoneyChangeReason.Trade => "PROPERTY TRADE",
                MoneyChangeReason.Rent => "RENT",
                MoneyChangeReason.Tax => "TAX",
                MoneyChangeReason.Card => "CARD",
                MoneyChangeReason.Building => "BUILDING",
                _ => "PURCHASE"
            };
            label.text = (delta > 0 ? "+$" : "−$") + System.Math.Abs((long)delta).ToString("N0") + "\n" + caption;
            label.EnableInClassList("is-gain", delta > 0);
            label.EnableInClassList("is-loss", delta <= 0);
            label.style.display = DisplayStyle.Flex;
            label.style.opacity = 1;
            label.BringToFront();
            visible = true;
            return true;
        }

        public void Track(Vector3 position, float elapsedFraction)
        {
            head = position;
            progress = Mathf.Clamp01(elapsedFraction);
        }

        void LateUpdate()
        {
            var camera = Camera.main;
            if (!visible || label?.panel == null || camera == null) return;
            if (camera.WorldToViewportPoint(head).z <= 0) { label.style.opacity = 0; return; }
            var point = RuntimePanelUtils.CameraTransformWorldToPanel(label.panel, head, camera);
            point = label.parent.WorldToLocal(point);
            label.style.left = Mathf.Clamp(point.x - 120, 8, Mathf.Max(8, label.parent.contentRect.width - 248));
            label.style.top = Mathf.Clamp(point.y - 90 - progress * 24, 8, Mathf.Max(8, label.parent.contentRect.height - 84));
            label.style.opacity = 1f - Mathf.InverseLerp(0.72f, 1f, progress);
        }

        public void Hide()
        {
            visible = false;
            if (label != null) label.style.display = DisplayStyle.None;
        }

        void OnDestroy()
        {
            label?.RemoveFromHierarchy();
            if (overlayObject != null) Destroy(overlayObject);
            if (panelSettings != null) Destroy(panelSettings);
        }
    }
}
