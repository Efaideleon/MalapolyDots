using System;
using UnityEngine.UIElements;

namespace Assets.Scripts.TitleScreen.NetworkUI.Panels
{
    public class MainMenuBackButton : IDisposable
    {
        private VisualElement _root;
        private Button _backButton; 

        public MainMenuBackButton(VisualElement root)
        {
            _root = root;
            _backButton = _root.Q<Button>();
        }

        public void Subcribe()
        {
            if (_backButton != null)
            {
                _backButton.clickable.clicked += HandleBackButtonClick;
            }
        }

        private void HandleBackButtonClick()
        {
            UnityEngine.Debug.Log("Back button clicked");
        }

        public void Dispose()
        {
            _backButton.clickable.clicked -= HandleBackButtonClick;
        }
    }
}