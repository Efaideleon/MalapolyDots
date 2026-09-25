using System;
using Assets.Common;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DOTS.GamePlay
{
    public class MatchRestart : MonoBehaviour
    {
        bool requested, restarting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var go = new GameObject("Match restart");
            DontDestroyOnLoad(go);
            var restart = go.AddComponent<MatchRestart>();
            NetworkRequests.ReturnToMainMenu = () => restart.requested = true;
        }

        async void Update()
        {
            if (!requested || restarting) return;
            requested = false;
            restarting = true;
            NetworkRequests.MenuReturnStatus = "Leaving the table…";
            try
            {
                if (NetworkRequests.LeaveOnlineSession != null)
                    await NetworkRequests.LeaveOnlineSession();
            }
            catch (Exception error)
            {
                Debug.LogWarning($"Leaving online session failed: {error.Message}");
                NetworkRequests.MenuReturnStatus = "Could not leave the online session. Check your connection and try again.";
                restarting = false;
                return;
            }
            try
            {
                string scene = SceneManager.GetActiveScene().path;
                NetworkRequests.StartHost = NetworkRequests.StartClient = NetworkRequests.StartGame = false;
                NetworkRequests.ExpectedLobbyPlayers = 0;
                NetworkRequests.GoBackToMainMenu = false;
                World.DisposeAllWorlds();
                DefaultWorldInitialization.Initialize("Default World");
                var load = SceneManager.LoadSceneAsync(scene);
                while (!load.isDone) await System.Threading.Tasks.Task.Yield();
            }
            catch (Exception error) { Debug.LogException(error); }
            finally { restarting = false; NetworkRequests.MenuReturnStatus = ""; }
        }
    }
}
