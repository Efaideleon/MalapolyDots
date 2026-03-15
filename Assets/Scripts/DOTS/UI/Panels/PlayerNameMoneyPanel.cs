using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public interface IAnimatablePanel
    {
        public int DisplayIndex { get; set; }
        public void AnimateTranslation(List<TimeValue> duration, StatsPanelPosition position);
    }

    public class Panel
    {
        public VisualElement Root { get; private set; }

        public Panel(VisualElement root)
        {
            Root = root;
        }

        public float ResolvedWidth => Root.resolvedStyle.width;
    }

    public class PlayerNameMoneyPanel : Panel, IEquatable<PlayerNameMoneyPanel> , IAnimatablePanel
    {
        public int ID { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }
        public VisualElement Icon { get; private set; }
        private readonly VisualElement _container;
        public int DisplayIndex { get; set; }

        public PlayerNameMoneyPanel(VisualElement root, int id) : base(root)
        {
            ID = id;
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
                return ID == other.ID;
            }
            return false;
        }

        public void AnimateTranslation(List<TimeValue> duration, StatsPanelPosition position)
        {
            Root.schedule.Execute((_) =>
            {
                Root.style.transitionDuration = duration;
                Root.style.translate = new Translate(position.Right, position.Top);
            }).ExecuteLater(0);
        }
    }

    public struct StatsPanelPosition
    {
        public float Top { get; private set; }
        public float Right { get; private set; }

        public StatsPanelPosition(float top, float right)
        {
            Top = top;
            Right = right;
        }
    }

}
