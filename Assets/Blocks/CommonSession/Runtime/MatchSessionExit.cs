using Assets.Common;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace Blocks.Sessions.Common
{
    public static class MatchSessionExit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Register()
        {
            NetworkRequests.LeaveOnlineSession = async () =>
            {
                if (UnityServices.State != ServicesInitializationState.Initialized) return;
                if (!MultiplayerService.Instance.Sessions.TryGetValue("default-session", out var session)) return;
                if (session.IsHost) await session.AsHost().DeleteAsync();
                else await session.LeaveAsync();
            };
        }
    }
}
