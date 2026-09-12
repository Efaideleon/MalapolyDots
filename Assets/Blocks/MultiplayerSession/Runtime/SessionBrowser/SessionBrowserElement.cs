using Blocks.Sessions.Common;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    [UxmlElement]
    public partial class SessionBrowserElement : VisualElement
    {
        readonly Label m_Status;
        readonly ScrollView m_List;
        readonly Button m_Refresh;
        SessionSettings m_Settings;
        SessionBrowserViewModel m_ViewModel;
        IVisualElementScheduledItem m_Poll;
        int m_MaxSessionsDisplayed = 20;

        [CreateProperty, UxmlAttribute]
        public SessionSettings SessionSettings
        {
            get => m_Settings;
            set
            {
                if (m_Settings == value) return;
                m_Settings = value;
                if (panel != null) Bind();
            }
        }

        [CreateProperty, UxmlAttribute]
        public int MaxSessionsDisplayed
        {
            get => m_MaxSessionsDisplayed;
            set => m_MaxSessionsDisplayed = Mathf.Clamp(value, 1, 100);
        }

        public SessionBrowserElement()
        {
            AddToClassList("open-lobbies");
            m_Status = new Label { enableRichText = false };
            m_Status.AddToClassList("open-lobbies-status");
            Add(m_Status);
            m_List = new ScrollView(ScrollViewMode.Vertical);
            m_List.AddToClassList("open-lobbies-list");
            Add(m_List);
            m_Refresh = new Button(Refresh) { text = "Refresh lobbies" };
            m_Refresh.AddToClassList("open-lobbies-refresh");
            Add(m_Refresh);
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Bind();
                m_Poll = schedule.Execute(RefreshIfVisible).Every(5000);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                m_Poll?.Pause();
                m_Poll = null;
                Cleanup();
            });
        }

        void Bind()
        {
            Cleanup();
            if (m_Settings == null)
            {
                m_Status.text = "Lobby settings are unavailable.";
                m_Refresh.SetEnabled(false);
                return;
            }
            m_ViewModel = new SessionBrowserViewModel(m_Settings.sessionType);
            m_ViewModel.Changed += Render;
            Render();
            RefreshIfVisible();
        }

        void RefreshIfVisible()
        {
            for (VisualElement element = this; element != null; element = element.parent)
                if (element.resolvedStyle.display == DisplayStyle.None) return;
            Refresh();
        }

        void Refresh()
        {
            if (m_ViewModel != null) _ = m_ViewModel.UpdateSessionListAsync(MaxSessionsDisplayed);
        }

        void Render()
        {
            if (m_ViewModel == null) return;
            m_Status.text = m_ViewModel.Status;
            m_Refresh.SetEnabled(m_ViewModel.CanRefresh);
            m_List.Clear();
            foreach (var session in m_ViewModel.Sessions)
            {
                string id = session.Id;
                var row = new Button(() => Join(id))
                {
                    text = $"{session.Name}     {session.MaxPlayers - session.AvailableSlots}/{session.MaxPlayers} players     Join",
                    enableRichText = false
                };
                row.AddToClassList("open-lobby-row");
                row.SetEnabled(m_ViewModel.CanJoin);
                m_List.Add(row);
            }
        }

        void Join(string id)
        {
            if (m_ViewModel != null && m_Settings != null)
                _ = m_ViewModel.JoinSessionAsync(id, m_Settings.ToJoinSessionOptions());
        }

        void Cleanup()
        {
            if (m_ViewModel == null) return;
            m_ViewModel.Changed -= Render;
            m_ViewModel.Dispose();
            m_ViewModel = null;
        }
    }
}
