using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class LobbyPanel : NetworkPanelBase
    {
        private readonly VisualElement _startContainer;
        private readonly VisualElement _backButtonContainer;
        private readonly Button StartButton;
        private readonly Button BackButton;

        public LobbyPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            UnityEngine.Debug.Log("[LobbyPanel] | Initializing Lobby panel.");
            _startContainer = root.Q<VisualElement>("purchase-btn-container");
            _backButtonContainer = root.Q<VisualElement>("back-btn-container");
            StartButton = _startContainer.Q<Button>("StartButton");
            BackButton = _backButtonContainer.Q<Button>("BackButton");
            if (BackButton == null)
            {
                UnityEngine.Debug.Log("BackButton is undefined");
            }
        }

        public override void Dispose()
        {
            UnsubscribeEvents();
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        public void HideStartButton()
        {
            StartButton.style.display = DisplayStyle.None;
            UnityEngine.Debug.Log($"[LobbyPanel] | Hiding StartButton");
        }

        private void SubscribeEvents()
        {
            StartButton.clickable.clicked += HandleStartButton;
            BackButton.clickable.clicked += HandleBackButton;
        }

        private void HandleStartButton()
        {
            UnityEngine.Debug.Log("[LobbyPanel] | Clicking Start button.");
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.LobbyStartButton });
        }

        private void HandleBackButton()
        {
            UnityEngine.Debug.Log("[LobbyPanel] | Clicking back button.");
            UIRequests.Enqueue(new UIRequest 
            { 
                Value = UIRequestType.Back
            });
        }
        
        private void UnsubscribeEvents()
        {
            StartButton.clickable.clicked -= HandleStartButton;
            BackButton.clickable.clicked -= HandleBackButton;
        }
    }
}
