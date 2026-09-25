using System;
using System.Threading.Tasks;
using Assets.Common;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Blocks.Sessions.Common.Assets.Blocks.CommonSession.Runtime.StartGameCode
{
    public class StartGameCodeViewModel : IDisposable
    {
        readonly SessionObserver m_Observer;
        ISession m_Session;
        bool m_Starting, m_StartRequested, m_Disposed;
        [CreateProperty] public bool CanStart => !m_Disposed && m_Session != null && m_Session.IsHost &&
            !m_Session.IsLocked && !m_Starting && !m_StartRequested;
        public string Status { get; private set; } = "Create or join a lobby.";
        public event Action Changed;

        public StartGameCodeViewModel(string sessionType)
        {
            m_Observer = new SessionObserver(sessionType);
            m_Observer.SessionAdded += OnSessionAdded;
            if (m_Observer.Session != null) OnSessionAdded(m_Observer.Session);
        }

        void OnSessionAdded(ISession session)
        {
            CleanupSession();
            m_StartRequested = false;
            m_Session = session;
            session.Changed += OnChanged;
            session.RemovedFromSession += OnRemoved;
            session.Deleted += OnRemoved;
            Status = session.IsHost ? "Up to 6 players. Start whenever you are ready." : "Waiting for the host to start character selection.";
            OnChanged();
        }

        void OnChanged()
        {
            if (m_Session?.IsHost == true && m_StartRequested)
                NetworkRequests.ExpectedLobbyPlayers = m_Session.Players.Count;
            Changed?.Invoke();
        }

        public async Task StartAsync()
        {
            if (!CanStart) return;
            var session = m_Session;
            m_Starting = true;
            Status = "Closing the lobby and starting character selection…";
            Changed?.Invoke();
            try
            {
                var host = session.AsHost();
                host.IsLocked = true;
                host.IsPrivate = true;
                await host.SavePropertiesAsync();
                await session.RefreshAsync();
                if (m_Disposed || m_Session != session || !session.IsHost) return;
                m_StartRequested = true;
                NetworkRequests.ExpectedLobbyPlayers = session.Players.Count;
                NetworkRequests.MatchRoundLimit = NetworkRequests.SelectedRoundLimit;
                NetworkRequests.StartGame = true;
                Status = "Choose a character. The game starts when everyone locks in.";
            }
            catch (Exception exception)
            {
                if (m_Disposed || m_Session != session) return;
                Status = "Could not start the lobby. Try again or leave and create another.";
                Debug.LogWarning($"Starting lobby failed: {exception.Message}");
                try
                {
                    var host = session.AsHost();
                    host.IsLocked = false;
                    host.IsPrivate = false;
                    await host.SavePropertiesAsync();
                }
                catch (Exception reopenError) { Debug.LogWarning($"Reopening lobby failed: {reopenError.Message}"); }
            }
            finally
            {
                m_Starting = false;
                if (!m_Disposed) Changed?.Invoke();
            }
        }

        void OnRemoved()
        {
            if (m_Session?.IsHost == true)
            {
                NetworkRequests.StartGame = false;
                NetworkRequests.ExpectedLobbyPlayers = 0;
            }
            CleanupSession();
            m_StartRequested = false;
            Status = "Create or join a lobby.";
            Changed?.Invoke();
        }

        void CleanupSession()
        {
            if (m_Session == null) return;
            m_Session.Changed -= OnChanged;
            m_Session.RemovedFromSession -= OnRemoved;
            m_Session.Deleted -= OnRemoved;
            m_Session = null;
        }

        public void Dispose()
        {
            m_Disposed = true;
            m_Observer.SessionAdded -= OnSessionAdded;
            m_Observer.Dispose();
            CleanupSession();
            Changed = null;
        }
    }
}
