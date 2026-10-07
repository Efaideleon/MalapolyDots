#nullable enable
using System;
using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class MainMenuPanel : NetworkPanelBase
    {
        private readonly Button HostButton;
        private readonly Button JoinButton;
        private readonly Button PublicLobbyButton;
        private readonly Button SoloButton;

        public MainMenuPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            PublicLobbyButton = root.Q<Button>("JoinPublicLobbyButton") ?? throw new InvalidOperationException("JoinPublicLobbyButton not found");
            HostButton = root.Q<Button>("CreateGameButton") ?? throw new InvalidOperationException("CreateGameButton not found");
            JoinButton = root.Q<Button>("EnterCodeButton") ?? throw new InvalidOperationException("EnterCodeButton not found");
            SoloButton = root.Q<Button>("SoloGameButton") ?? throw new InvalidOperationException("SoloGameButton not found");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        public override void Dispose()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            HostButton.clickable.clicked += HandleHostButton;
            JoinButton.clickable.clicked += HandleJoinButton;
            PublicLobbyButton.clickable.clicked += HandlePublicLobbyButton;
            SoloButton.clicked += HandleSoloButton;
        }

        public void HandlePublicLobbyButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.PlayButton });
        }

        void HandleSoloButton() => UIRequests.Enqueue(new UIRequest { Value = UIRequestType.MainMenuSolo });

        public void HandleHostButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.MainMenuHost });
        }

        public void HandleJoinButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.MainMenuJoin });
        }

        private void UnsubscribeEvents()
        {
            HostButton.clickable.clicked -= HandleHostButton;
            JoinButton.clickable.clicked -= HandleJoinButton;
            PublicLobbyButton.clickable.clicked -= HandlePublicLobbyButton;
            SoloButton.clicked -= HandleSoloButton;
        }
    }
}
