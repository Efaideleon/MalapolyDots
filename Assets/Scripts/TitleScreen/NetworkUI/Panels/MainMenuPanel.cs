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
        private readonly Button PlayButton;

        public MainMenuPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            PlayButton = root.Q<Button>("PlayButton") ?? throw new InvalidOperationException("PlayButton not found");
            HostButton = root.Q<Button>("HostButton") ?? throw new InvalidOperationException("HostButton not found");
            JoinButton = root.Q<Button>("JoinButton") ?? throw new InvalidOperationException("JoinButton not found");
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
            PlayButton.clickable.clicked += HandlePlayButton;
        }

        public void HandlePlayButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.PlayButton });
        }

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
            PlayButton.clickable.clicked -= HandlePlayButton;
        }
    }
}
