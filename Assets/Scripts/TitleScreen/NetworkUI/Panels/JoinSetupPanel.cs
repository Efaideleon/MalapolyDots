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

        public JoinSetupPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            JoinButton = root.Q<Button>("JoinButton") ?? throw new InvalidOperationException("JoinButton not found");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            JoinButton.clickable.clicked += HandleJoinButton;
        }

        private void UnsubscribeEvents()
        {
            JoinButton.clickable.clicked -= HandleJoinButton;
        }

        private void HandleJoinButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.JoinSetupJoin });
        }

        public override void Dispose()
        { 
            UnsubscribeEvents();
        }
    }
}
