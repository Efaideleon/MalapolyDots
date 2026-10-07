using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.GamePlay
{
    public sealed class DiceResultOverlay : MonoBehaviour
    {
        UIDocument document;
        PanelSettings settings;
        Label label;

        public void Show(int first, int second)
        {
            if (document == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                settings.name = "Dice result panel";
                settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                settings.referenceResolution = new Vector2Int(1920, 1080);
                settings.match = .5f;
                settings.sortingOrder = 950;
                var panel = new GameObject("Dice result overlay");
                panel.transform.SetParent(transform, false);
                document = panel.AddComponent<UIDocument>();
                document.panelSettings = settings;
            }
            var root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute;
            root.style.left = root.style.right = root.style.top = root.style.bottom = 0;
            if (label == null || label.parent != root)
            {
                label = new Label { name = "dice-result", pickingMode = PickingMode.Ignore };
                label.style.position = Position.Absolute;
                label.style.left = Length.Percent(30);
                label.style.right = Length.Percent(30);
                label.style.top = Length.Percent(64);
                label.style.height = 76;
                var theme = Resources.Load<StyleSheet>("UI/GameTheme");
                if (theme != null && !root.styleSheets.Contains(theme)) root.styleSheets.Add(theme);
                label.AddToClassList("game-dice-result");
                root.Add(label);
            }
            label.text = $"{first} + {second} = {first + second}";
            label.style.display = DisplayStyle.Flex;
        }

        public void Hide() { if (label != null) label.style.display = DisplayStyle.None; }
        void OnDestroy()
        {
            if (settings == null) return;
            if (Application.isPlaying) Destroy(settings);
            else DestroyImmediate(settings);
        }
    }
}
