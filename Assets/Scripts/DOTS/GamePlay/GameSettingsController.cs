using Assets.Common;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay.CameraSystems;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DOTS.GamePlay
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameSettingsController : MonoBehaviour
    {
        static GameSettingsController instance;
        public static bool IsOpen { get; private set; }
        public static bool HasActiveMatch { get; private set; }
        public static int ConsumedEscapeFrame { get; private set; } = -1;
        PanelSettings panelSettings;
        UIDocument document;
        VisualElement root, safeRegion, shade, quitShade;
        Toggle muted, inverted, reducedMotion;
        Slider volume, sensitivity, zoom;
        Label volumeValue, cameraValue, zoomValue, playStatus, quitWarning, quitStatus;
        Button quitButton, resetCamera, confirmQuit, cancelQuit;
        bool inMatch, runningMatch, wasInMatch, host, ownsPause, leaving;
        Rect previousSafeArea;
        Vector2Int previousSize;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntime() { instance = null; IsOpen = HasActiveMatch = false; ConsumedEscapeFrame = -1; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance != null) return;
            var go = new GameObject("Game settings");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameSettingsController>();
        }

        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            GamePreferences.Apply();
            var asset = Resources.Load<VisualTreeAsset>("UI/GameSettings");
            if (asset == null) return;
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1280, 720);
            panelSettings.match = 0.5f;
            panelSettings.sortingOrder = 20000;
            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = asset;
            ReuseGameTheme();
            BindControls();
        }

        void ReuseGameTheme()
        {
            if (panelSettings.themeStyleSheet != null) return;
            foreach (var other in Object.FindObjectsByType<UIDocument>())
                if (other != document && other.panelSettings != null && other.panelSettings.themeStyleSheet != null)
                {
                    panelSettings.themeStyleSheet = other.panelSettings.themeStyleSheet;
                    panelSettings.textSettings = other.panelSettings.textSettings;
                    break;
                }
        }

        void BindControls()
        {
            bool showConfirmation = quitShade != null && quitShade.ClassListContains("is-visible");
            root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.AddToClassList("settings-document");
            safeRegion = root.Q("safeRegion");
            shade = root.Q("settingsShade");
            quitShade = root.Q("quitShade");
            muted = root.Q<Toggle>("muteAudio");
            inverted = root.Q<Toggle>("invertCamera");
            reducedMotion = root.Q<Toggle>("reduceMotion");
            volume = root.Q<Slider>("audioVolume");
            sensitivity = root.Q<Slider>("cameraSensitivity");
            zoom = root.Q<Slider>("zoomSensitivity");
            volumeValue = root.Q<Label>("volumeValue");
            cameraValue = root.Q<Label>("cameraValue");
            zoomValue = root.Q<Label>("zoomValue");
            playStatus = root.Q<Label>("playStatus");
            quitWarning = root.Q<Label>("quitWarning");
            quitStatus = root.Q<Label>("quitStatus");
            quitButton = root.Q<Button>("quitMatch");
            resetCamera = root.Q<Button>("resetCamera");
            confirmQuit = root.Q<Button>("confirmQuit");
            cancelQuit = root.Q<Button>("cancelQuit");
            root.Q<Button>("openSettings").clicked += Open;
            root.Q<Button>("closeSettings").clicked += Close;
            muted.RegisterValueChangedCallback(evt => { GamePreferences.Current.Muted = evt.newValue; ApplyChanges(); });
            volume.RegisterValueChangedCallback(evt => { GamePreferences.Current.Volume = evt.newValue; ApplyChanges(); });
            sensitivity.RegisterValueChangedCallback(evt => { GamePreferences.Current.CameraSensitivity = evt.newValue; ApplyChanges(); });
            zoom.RegisterValueChangedCallback(evt => { GamePreferences.Current.ZoomSensitivity = evt.newValue; ApplyChanges(); });
            inverted.RegisterValueChangedCallback(evt => { GamePreferences.Current.InvertVertical = evt.newValue; ApplyChanges(); });
            reducedMotion.RegisterValueChangedCallback(evt => { GamePreferences.Current.ReducedCameraMotion = evt.newValue; ApplyChanges(); });
            root.Q<Button>("resetSettings").clicked += () => { GamePreferences.ResetDefaults(); RefreshValues(); };
            resetCamera.clicked += () => CinemachineCameraManager.Instance?.ResetPlayerView();
            quitButton.clicked += ShowQuitConfirmation;
            cancelQuit.clicked += CancelQuit;
            confirmQuit.clicked += ConfirmQuit;
            root.Q<Label>("cameraInstructions").text =
                "Two-finger drag to rotate; pinch to zoom. With a mouse, right-drag to rotate and scroll to zoom.";
            RefreshValues();
            shade.EnableInClassList("is-visible", IsOpen);
            quitShade.EnableInClassList("is-visible", showConfirmation);
            previousSize = default;
        }

        void Update()
        {
            if (document == null) return;
            ReuseGameTheme();
            if (root != document.rootVisualElement || root.Q("settingsShade") != shade) BindControls();
            ReadMatchContext();
            HasActiveMatch = inMatch;
            if (wasInMatch && !inMatch && IsOpen) { leaving = false; Close(); }
            wasInMatch = inMatch;
            quitButton.EnableInClassList("is-visible", inMatch);
            resetCamera.SetEnabled(inMatch && CinemachineCameraManager.Instance != null);
            UpdateSafeArea();
            if (!IsOpen) return;
            playStatus.text = !inMatch ? "Make yourself comfortable before joining a table." : !runningMatch ? "The match has finished." :
                SoloSession.Active ? "Your solo match is paused." : "Online play continues while settings are open.";
            if (leaving)
            {
                quitStatus.text = NetworkRequests.MenuReturnStatus;
                if (quitStatus.text.StartsWith("Could not"))
                {
                    leaving = false;
                    confirmQuit.SetEnabled(true);
                    cancelQuit.SetEnabled(true);
                }
            }
            if (!leaving && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                ConsumedEscapeFrame = Time.frameCount;
                if (quitShade.ClassListContains("is-visible")) CancelQuit(); else Close();
            }
        }

        void ReadMatchContext()
        {
            inMatch = runningMatch = host = false;
            var client = ClientServerBootstrap.ClientWorld;
            if (client == null || !client.IsCreated) return;
            using var gameQuery = client.EntityManager.CreateEntityQuery(typeof(GameStateComponent));
            if (gameQuery.CalculateEntityCount() == 1)
            {
                var game = gameQuery.GetSingleton<GameStateComponent>();
                inMatch = game.AllPlacesInstantiated;
                runningMatch = inMatch && game.State != GameState.GameOver;
            }
            var server = ClientServerBootstrap.ServerWorld;
            if (!inMatch || SoloSession.Active || server == null || !server.IsCreated) return;
            using var driver = server.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver));
            host = driver.CalculateEntityCount() == 1 && driver.GetSingleton<NetworkStreamDriver>().DriverStore.HasListeningInterfaces;
        }

        public void Open()
        {
            if (shade == null || IsOpen) return;
            ReadMatchContext();
            ownsPause = inMatch && SoloSession.Active && SoloPauseController.PauseForSettings();
            IsOpen = true;
            leaving = false;
            quitShade.RemoveFromClassList("is-visible");
            shade.AddToClassList("is-visible");
            RefreshValues();
            root.Q<Button>("closeSettings").Focus();
        }

        public void Close()
        {
            if (leaving) return;
            GamePreferences.Save();
            IsOpen = false;
            shade?.RemoveFromClassList("is-visible");
            quitShade?.RemoveFromClassList("is-visible");
            SoloPauseController.FinishSettingsPause(ownsPause);
            ownsPause = false;
            root?.Q<Button>("openSettings")?.Focus();
        }

        void ApplyChanges() { GamePreferences.Apply(); RefreshValues(); }

        void RefreshValues()
        {
            var data = GamePreferences.Current;
            muted.SetValueWithoutNotify(data.Muted);
            volume.SetValueWithoutNotify(data.Volume);
            sensitivity.SetValueWithoutNotify(data.CameraSensitivity);
            zoom.SetValueWithoutNotify(data.ZoomSensitivity);
            inverted.SetValueWithoutNotify(data.InvertVertical);
            reducedMotion.SetValueWithoutNotify(data.ReducedCameraMotion);
            volumeValue.text = Mathf.RoundToInt(data.Volume * 100f) + "%";
            cameraValue.text = data.CameraSensitivity.ToString("0.00") + "×";
            zoomValue.text = data.ZoomSensitivity.ToString("0.00") + "×";
            volume.SetEnabled(!data.Muted);
        }

        public static string QuitMessage(bool solo, bool hosting) => solo
            ? "Your current solo match will be lost. Return to the main menu?"
            : hosting ? "You are hosting this table. Quitting will close the match for everyone. Return to the main menu?"
            : "You will leave this match and cannot rejoin it in progress. Return to the main menu?";

        void ShowQuitConfirmation()
        {
            ReadMatchContext();
            if (!inMatch) return;
            quitWarning.text = QuitMessage(SoloSession.Active, host);
            quitStatus.text = "";
            confirmQuit.SetEnabled(NetworkRequests.ReturnToMainMenu != null);
            cancelQuit.SetEnabled(true);
            quitShade.AddToClassList("is-visible");
            cancelQuit.Focus();
        }

        void CancelQuit()
        {
            if (leaving) return;
            quitShade.RemoveFromClassList("is-visible");
            quitButton.Focus();
        }

        void ConfirmQuit()
        {
            if (leaving || NetworkRequests.ReturnToMainMenu == null) return;
            GamePreferences.Save();
            leaving = true;
            quitStatus.text = "Leaving the table…";
            confirmQuit.SetEnabled(false);
            cancelQuit.SetEnabled(false);
            NetworkRequests.ReturnToMainMenu.Invoke();
        }

        void UpdateSafeArea()
        {
            var size = new Vector2Int(Screen.width, Screen.height);
            var area = Screen.safeArea;
            if (size == previousSize && area == previousSafeArea) return;
            previousSize = size;
            previousSafeArea = area;
            if (size.x <= 0 || size.y <= 0) return;
            safeRegion.style.left = Length.Percent(area.xMin / size.x * 100f);
            safeRegion.style.right = Length.Percent((size.x - area.xMax) / size.x * 100f);
            safeRegion.style.top = Length.Percent((size.y - area.yMax) / size.y * 100f);
            safeRegion.style.bottom = Length.Percent(area.yMin / size.y * 100f);
        }

        void OnApplicationPause(bool paused) { if (paused) GamePreferences.Save(); }
        void OnApplicationFocus(bool focused) { if (!focused) GamePreferences.Save(); }
        void OnApplicationQuit() => GamePreferences.Save();
        void OnDestroy()
        {
            if (instance == this)
            {
                SoloPauseController.FinishSettingsPause(ownsPause);
                instance = null;
                IsOpen = HasActiveMatch = false;
            }
            if (panelSettings != null) Destroy(panelSettings);
        }
    }
}
