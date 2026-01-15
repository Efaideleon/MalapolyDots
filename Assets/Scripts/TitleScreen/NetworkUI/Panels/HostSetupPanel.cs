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
        private readonly Button StartHostingButton; 

        public HostSetupPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            NumberOfPlayersDropdownField = root.Q<DropdownField>("NumOfPlayersDropdownField") ?? throw new InvalidOperationException("NumOfPlayersDropdownField not found");
            NumberOfRoundsDropdownField = root.Q<DropdownField>("NumOfRoundsDropdownField") ?? throw new InvalidOperationException("NumOfRoundsDropdownField not found");
            StartHostingButton = root.Q<Button>("StartHostingButton") ?? throw new InvalidOperationException("StartHostingButton not found");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            StartHostingButton.clickable.clicked += HandleStartHosting;
        }

        private void UnsubscribeEvents()
        {
            StartHostingButton.clickable.clicked -= HandleStartHosting;
        }

        private void HandleStartHosting()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.HostSetupHost });
        }

        public override void Dispose()
        { 
            UnsubscribeEvents();
        }
    }
}
