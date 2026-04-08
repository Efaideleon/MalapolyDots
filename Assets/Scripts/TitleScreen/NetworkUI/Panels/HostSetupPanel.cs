using System;
using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class HostSetupPanel : NetworkPanelBase
    {
        private readonly DropdownField NumberOfPlayersDropdownField; 
        private readonly DropdownField NumberOfRoundsDropdownField; 
        private readonly Button BackButton; 
        private readonly Button StartHostingButton; 

        public HostSetupPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            NumberOfPlayersDropdownField = root.Q<DropdownField>("NumOfPlayersDropdownField") ?? throw new InvalidOperationException("NumOfPlayersDropdownField not found");
            NumberOfRoundsDropdownField = root.Q<DropdownField>("NumOfRoundsDropdownField") ?? throw new InvalidOperationException("NumOfRoundsDropdownField not found");
            StartHostingButton = root.Q<Button>("StartHostingButton") ?? throw new InvalidOperationException("StartHostingButton not found");
            BackButton = root.Q<Button>("BackButton") ?? throw new InvalidOperationException("BackButton not found");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            StartHostingButton.clickable.clicked += HandleStartHosting;
            BackButton.clickable.clicked += HandleBackButton;
        }

        private void UnsubscribeEvents()
        {
            StartHostingButton.clickable.clicked -= HandleStartHosting;
            BackButton.clickable.clicked -= HandleBackButton;
        }

        private void HandleStartHosting()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.HostSetupHost });
        }

        private void HandleBackButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.BackToMainMenu });
            UnityEngine.Debug.Log("[HostSetupPanel] | BackButton Pressed.");
        }

        public override void Dispose()
        { 
            UnsubscribeEvents();
        }
    }
}
