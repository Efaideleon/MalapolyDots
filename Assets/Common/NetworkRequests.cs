namespace Assets.Common
{
    public static class NetworkRequests
    {
        public static int SelectedRoundLimit = 8;
        public static int MatchRoundLimit = 8;
        public static System.Func<System.Threading.Tasks.Task> LeaveOnlineSession;
        public static System.Action ReturnToMainMenu;
        public static string MenuReturnStatus = "";
        public static bool StartHost;
        public static bool StartClient;
        public static bool StartGame;
        public static int ExpectedLobbyPlayers;
        public static int LobbyVersion;
        public static bool GoBackToMainMenu;
    }
}
