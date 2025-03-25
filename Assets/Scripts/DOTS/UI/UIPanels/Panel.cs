using UnityEngine.UIElements;
using Unity.Entities;
using System;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public class Panel 
    {
        public VisualElement Root { get; private set; }
        public Label TopLabel { get; private set; }
        public Button AcceptButton { get; private set; }

        public Action OnAcceptButton = null;

        public Panel(VisualElement root)
        {
            Root = root;
        }

        public void UpdateLabelReference(string className)
        {
            TopLabel = Root.Q<Label>(className);
        }

        public void UpdateAcceptButtonReference(string className)
        {
            AcceptButton = Root.Q<Button>(className);
        }

        public void UpdateLabelText(string text)
        {
            TopLabel.text = text;
        }

        public virtual void AddAcceptButtonAction(EntityQuery entityQuery) { }

        private void UnsubscribeAcceptButton()
        {
            if (OnAcceptButton != null)
            {
                AcceptButton.clickable.clicked -= OnAcceptButton;
            }
        }

        public virtual void Show() => Root.style.display = DisplayStyle.Flex;
        public virtual void Hide() => Root.style.display = DisplayStyle.None;

        public virtual void Dispose() => UnsubscribeAcceptButton();
    }
}
