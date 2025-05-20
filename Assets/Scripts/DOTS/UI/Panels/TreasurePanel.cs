using System;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class TreasurePanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }
        public TreasurePanel(VisualElement parent) 
        {
            Root = parent.Q<VisualElement>("TreasurePanel") ?? throw new ArgumentNullException($"[TreasurePanel] {nameof(parent)}");
            OkButton = Root.Q<Button>("treasure-panel-button") ?? throw new NullReferenceException($"[TreasurePanel] \"treasure-panel-button\" is missing");
            TitleLabel = Root.Q<Label>("treasure-panel-label") ?? throw new NullReferenceException($"[TreasurePanel] \"treasure-panel-label\" is missing");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}
