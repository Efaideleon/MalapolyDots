using System.Collections.Generic;
using Assets.Common;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public class SoloSetupPanel : NetworkPanelBase
    {
        readonly DropdownField opponents, rounds;
        readonly Button start, back;
        readonly Label status;
        IVisualElementScheduledItem refresh;

        public SoloSetupPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            opponents = root.Q<DropdownField>("AiOpponents");
            rounds = root.Q<DropdownField>("SoloRounds");
            start = root.Q<Button>("StartSoloButton");
            back = root.Q<Button>("SoloBackButton");
            status = root.Q<Label>("SoloStatus");
        }

        public override void Initialize()
        {
            opponents.SetValueWithoutNotify(SoloSession.Opponents.ToString());
            rounds.SetValueWithoutNotify(NetworkRequests.SelectedRoundLimit.ToString());
            start.clicked += StartSolo;
            back.clicked += Back;
            refresh = status.schedule.Execute(() =>
            {
                status.text = SoloSession.Status;
                start.SetEnabled(!SoloSession.StartRequested && !SoloSession.Active);
                back.SetEnabled(!SoloSession.StartRequested && !SoloSession.Active);
            }).Every(100);
        }

        void StartSolo()
        {
            SoloSession.Opponents = int.Parse(opponents.value);
            NetworkRequests.SelectedRoundLimit = int.Parse(rounds.value);
            SoloSession.Status = "Setting up your table…";
            SoloSession.StartRequested = true;
            start.SetEnabled(false);
            back.SetEnabled(false);
        }

        void Back() => UIRequests.Enqueue(new UIRequest { Value = UIRequestType.BackToMainMenu });

        public override void Dispose()
        {
            start.clicked -= StartSolo;
            back.clicked -= Back;
            refresh?.Pause();
        }
    }
}
