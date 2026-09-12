using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Common;
using Blocks.Sessions.Common;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Blocks.Sessions
{
    public class SessionBrowserViewModel : IDisposable
    {
        readonly SessionObserver m_Observer;
        readonly ServiceObserver<IMultiplayerService> m_ServiceObserver;
        ISession m_Session;
        bool m_Querying, m_Joining, m_Disposed;

        public List<SessionInfoViewModel> Sessions { get; private set; } = new();
        public string Status { get; private set; } = "Connecting to online lobbies…";
        public bool CanRefresh => !m_Disposed && m_ServiceObserver?.Service != null &&
            m_Session == null && !m_Querying && !m_Joining;
        public bool CanJoin => CanRefresh;
        public event Action Changed;

        public SessionBrowserViewModel(string sessionType)
        {
            m_Observer = new SessionObserver(sessionType);
            m_Observer.SessionAdded += OnSessionAdded;
            if (UnityServices.Instance != null)
                m_ServiceObserver = new ServiceObserver<IMultiplayerService>();
            if (m_Observer.Session != null) OnSessionAdded(m_Observer.Session);
        }

        public static QuerySessionsOptions OpenLobbyQuery(int count) => new()
        {
            Count = Math.Max(1, Math.Min(count, 100)),
            FilterOptions = new List<FilterOption>
            {
                new(FilterField.AvailableSlots, "0", FilterOperation.Greater),
                new(FilterField.IsLocked, "false", FilterOperation.Equal),
                new(FilterField.HasPassword, "false", FilterOperation.Equal),
                new(FilterField.StringIndex1, SessionSettings.PublicLobbyKind, FilterOperation.Equal)
            },
            SortOptions = new List<SortOption> { new(SortOrder.Descending, SortField.CreationTime) }
        };

        internal async Task UpdateSessionListAsync(int count)
        {
            if (!CanRefresh) return;
            m_Querying = true;
            Status = "Refreshing open lobbies…";
            Changed?.Invoke();
            try
            {
                var result = await MultiplayerService.Instance.QuerySessionsAsync(OpenLobbyQuery(count));
                if (m_Disposed || m_Session != null) return;
                ClearSessions();
                foreach (var session in result.Sessions)
                    if (!session.IsLocked && !session.HasPassword && session.AvailableSlots > 0)
                        Sessions.Add(new SessionInfoViewModel(session));
                Status = Sessions.Count == 0 ? "No open lobbies. Create one or refresh." : "Click a lobby to join. No code required.";
            }
            catch (Exception exception)
            {
                if (!m_Disposed && m_Session == null)
                {
                    ClearSessions();
                    Status = "Could not load lobbies. Check your connection and refresh.";
                    Debug.LogWarning($"Lobby refresh failed: {exception.Message}");
                }
            }
            finally
            {
                m_Querying = false;
                if (!m_Disposed) Changed?.Invoke();
            }
        }

        public async Task JoinSessionAsync(string sessionId, JoinSessionOptions options)
        {
            if (!CanJoin || string.IsNullOrEmpty(sessionId)) return;
            m_Joining = true;
            Status = "Joining lobby…";
            Changed?.Invoke();
            try
            {
                options.Password = null;
                await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, options);
                // The session observer also handles create/join from the other controls.
            }
            catch (Exception exception)
            {
                if (!m_Disposed)
                {
                    ClearSessions();
                    Status = "Could not join. The lobby may be full, closed, or already starting. Refresh to try again.";
                    Debug.LogWarning($"Lobby join failed: {exception.Message}");
                }
            }
            finally
            {
                m_Joining = false;
                if (!m_Disposed) Changed?.Invoke();
            }
        }

        void OnSessionAdded(ISession session)
        {
            if (m_Disposed) return;
            CleanupSession();
            m_Session = session;
            session.RemovedFromSession += OnSessionRemoved;
            session.Deleted += OnSessionRemoved;
            ClearSessions();
            Status = session.IsHost ? "Lobby open. Start whenever you are ready." : "Joined lobby. Waiting for the host to start character selection.";
            if (!session.IsHost) NetworkRequests.StartClient = true;
            Changed?.Invoke();
        }

        void OnSessionRemoved()
        {
            CleanupSession();
            Status = "Lobby closed or left. Choose another open lobby.";
            Changed?.Invoke();
        }

        void ClearSessions()
        {
            foreach (var session in Sessions) session.Dispose();
            Sessions = new List<SessionInfoViewModel>();
        }

        void CleanupSession()
        {
            if (m_Session == null) return;
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
        }

        public void Dispose()
        {
            m_Disposed = true;
            m_Observer.SessionAdded -= OnSessionAdded;
            m_Observer.Dispose();
            m_ServiceObserver?.Dispose();
            CleanupSession();
            ClearSessions();
            Changed = null;
        }
    }
}
