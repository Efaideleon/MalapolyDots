using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PlayerNameMoneyPanel : IEquatable<PlayerNameMoneyPanel>
    {
        public VisualElement Root { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }
        public VisualElement Icon { get; private set; }
        private readonly VisualElement _container;

        public PlayerNameMoneyPanel(VisualElement root)
        {
            Root = root;
            Root.name = "PlayerNameMoneyPanel";
            if (Root == null)
            {
                UnityEngine.Debug.Log("[PlayerNameMoneyPanel] | Stats panels parent is null");
            }

            Root.style.width = StyleKeyword.Auto;
            Root.style.height = StyleKeyword.Auto;
            Root.pickingMode = PickingMode.Ignore;
            Icon = Root.Q<VisualElement>("player-picture");
            PlayerNameLabel = Root.Q<Label>("player-name");
            PlayerMoneyLabel = Root.Q<Label>("player-money");
            _container = Root.Q<VisualElement>("player-panel");

            Root.AddToClassList("default-uxml");
            AddAnimationProperties();
            DisableHighlightPanel();
        }

        public bool IsOnScreen => Root.resolvedStyle.width > 0;

        public void SetName(FixedString64Bytes text)
        {
            PlayerNameLabel.text = text.ToString();
        }

        public void SetMoney(FixedString64Bytes text)
        {
            PlayerMoneyLabel.text = text.ToString();
        }

        private void AddAnimationProperties()
        {
            Root.style.transitionProperty = new List<StylePropertyName> { "translate" };
            Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
            Root.style.transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseInOut) };
        }

        public void HighlightPanel()
        {
            _container.AddToClassList("current-player-panel");
        }

        public void DisableHighlightPanel()
        {
            _container.RemoveFromClassList("current-player-panel");
        }

        public void SetSprite(Sprite sprite)
        {
            Icon.style.backgroundImage = new StyleBackground(sprite);
        }

        public bool Equals(PlayerNameMoneyPanel other)
        {
            if (other != null)
            {
                UnityEngine.Debug.Log($"[PlayerNameMoneyPanel] | comparing : {PlayerNameLabel.text} to {other.PlayerNameLabel.text}");
                return PlayerNameLabel.text.Equals(other.PlayerNameLabel.text);
            }
            return false;
        }
    }
}
