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
                SoloPauseController.ResumeCurrent();
                if (!SoloSession.Active && NetworkRequests.LeaveOnlineSession != null)
                {
                    var leave = NetworkRequests.LeaveOnlineSession();
                    if (await System.Threading.Tasks.Task.WhenAny(leave, System.Threading.Tasks.Task.Delay(10000)) == leave)
                        await leave;
                    else
                    {
                        Debug.LogWarning("The online session did not respond. Returning to the main menu.");
                        _ = leave.ContinueWith(task => Debug.LogWarning(task.Exception),
                            System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
            catch (Exception error)
            {
                Debug.LogWarning($"Leaving online session failed: {error.Message}");
            }
            try
            {
                string scene = SceneManager.GetActiveScene().path;
                NetworkRequests.StartHost = NetworkRequests.StartClient = NetworkRequests.StartGame = false;
                NetworkRequests.ExpectedLobbyPlayers = 0;
                NetworkRequests.GoBackToMainMenu = false;
                SoloSession.Reset();
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
