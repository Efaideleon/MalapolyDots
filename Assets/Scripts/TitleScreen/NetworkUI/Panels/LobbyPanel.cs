using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class LobbyPanel : NetworkPanelBase
    {
        private readonly Button StartButton;

        public LobbyPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            StartButton = root.Q<Button>("StartButton");
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
        }

        private void HandleStartButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.LobbyStartButton });
        }

        private void UnsubscribeEvents()
        {
            StartButton.clickable.clicked -= HandleStartButton;
        }
    }
}
