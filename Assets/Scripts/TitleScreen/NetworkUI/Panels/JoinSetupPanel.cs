using System;
using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class JoinSetupPanel : NetworkPanelBase
    {
        private readonly TextField PortTextField;
        private readonly Button JoinButton;
        private readonly Button BackButton;

        public JoinSetupPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            JoinButton = root.Q<Button>("JoinButton") ?? throw new InvalidOperationException("JoinButton not found");
            BackButton = root.Q<Button>("BackButton") ?? throw new InvalidOperationException("BackButton not found");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            JoinButton.clickable.clicked += HandleJoinButton;
            BackButton.clickable.clicked += HandleBackButton;
        }

        private void UnsubscribeEvents()
        {
            JoinButton.clickable.clicked -= HandleJoinButton;
            BackButton.clickable.clicked -= HandleBackButton;
        }

        private void HandleJoinButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.JoinSetupJoin });
        }

        private void HandleBackButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.BackToMainMenu });
            UnityEngine.Debug.Log("[JoinSetupPanel] | BackButton pressed.");
        }

        public override void Dispose()
        { 
            UnsubscribeEvents();
        }
    }
}
