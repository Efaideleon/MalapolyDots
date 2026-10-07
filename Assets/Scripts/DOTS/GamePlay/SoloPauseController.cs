using System;
using System.Collections.Generic;
using Assets.Common;
using Assets.Scripts.DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DOTS.GamePlay
{
    public sealed class SoloPauseState : IDisposable
    {
        readonly List<World> suspendedWorlds = new();
        float previousTimeScale;
        bool previousAudioPause;
        public bool IsPaused { get; private set; }

        public void Pause(IEnumerable<World> worlds)
        {
            if (IsPaused) return;
            IsPaused = SoloSession.Paused = true;
            previousTimeScale = Time.timeScale;
            previousAudioPause = AudioListener.pause;
            Time.timeScale = 0;
            AudioListener.pause = true;
            foreach (var world in worlds)
            {
                if (!world.IsCreated) continue;
                world.EntityManager.CompleteAllTrackedJobs();
                if (!ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world)) continue;
                suspendedWorlds.Add(world);
                // Suspending the world preserves system lifetimes and UI subscriptions on resume.
                ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
            }
        }

        public void Resume()
        {
            if (!IsPaused) return;
            foreach (var world in suspendedWorlds)
                if (world.IsCreated && !ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world))
                    ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
            suspendedWorlds.Clear();
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioPause;
            IsPaused = SoloSession.Paused = false;
        }

        public void Dispose() => Resume();
    }

    public sealed class SoloPauseController : MonoBehaviour
    {
        static SoloPauseController instance;
        readonly SoloPauseState pause = new();
        GameObject overlay;
        PanelSettings settings;
        UIDocument document;
        VisualElement root, modal, exitConfirmation;
        bool inGame;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSession() { instance = null; SoloSession.Reset(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance != null) return;
            var go = new GameObject("Solo pause controls");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SoloPauseController>();
        }

        public static void ResumeCurrent() => instance?.Resume();
        public static void PauseCurrent() => instance?.Pause();

        public static bool PauseForSettings()
        {
            if (instance == null) return false;
            instance.ReadMatchContext();
            bool alreadyPaused = instance.pause.IsPaused;
            instance.Pause();
            instance.modal?.RemoveFromClassList("is-visible");
            return !alreadyPaused && instance.pause.IsPaused;
        }

        public static void FinishSettingsPause(bool resume)
        {
            if (instance == null) return;
            if (resume) instance.Resume();
            else if (instance.pause.IsPaused) instance.modal?.AddToClassList("is-visible");
        }

        void Update()
        {
            ReadMatchContext();
            if (!inGame)
            {
                Resume();
                if (root != null) root.style.display = DisplayStyle.None;
                return;
            }
            if (root == null) CreateControls();
            if (root == null) return;
            if (root.Q("PauseModal") != modal) BindControls();
            root.style.display = DisplayStyle.Flex;
            if (!GameSettingsController.IsOpen && GameSettingsController.ConsumedEscapeFrame != Time.frameCount &&
                Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (pause.IsPaused) Resume(); else Pause();
            }
        }

        void ReadMatchContext()
        {
            inGame = false;
            var world = ClientServerBootstrap.ClientWorld;
            if (SoloSession.Active && world != null && world.IsCreated)
            {
                using var query = world.EntityManager.CreateEntityQuery(typeof(GameStateComponent));
                if (query.CalculateEntityCount() == 1)
                {
                    var game = query.GetSingleton<GameStateComponent>();
                    inGame = game.AllPlacesInstantiated && game.State != GameState.GameOver;
                }
            }
        }

        void CreateControls()
        {
            var asset = Resources.Load<VisualTreeAsset>("UI/SoloPause");
            if (asset == null) return;
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.match = 0.5f;
            settings.sortingOrder = 10000;
            overlay = new GameObject("Solo pause overlay");
            overlay.transform.SetParent(transform, false);
            document = overlay.AddComponent<UIDocument>();
            document.panelSettings = settings;
            document.visualTreeAsset = asset;
            BindControls();
        }

        void BindControls()
        {
            root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            var sheet = Resources.Load<StyleSheet>("UI/SoloMode");
            if (sheet != null && !root.styleSheets.Contains(sheet)) root.styleSheets.Add(sheet);
            root.AddToClassList("solo-document-root");
            modal = root.Q("PauseModal");
            exitConfirmation = root.Q("SoloExitConfirmation");
            root.Q<Button>("PauseSoloButton").clicked += Pause;
            root.Q<Button>("ResumeSoloButton").clicked += Resume;
            root.Q<Button>("LeaveSoloButton").clicked += () => exitConfirmation.AddToClassList("is-visible");
            root.Q<Button>("CancelLeaveSoloButton").clicked += () => exitConfirmation.RemoveFromClassList("is-visible");
            root.Q<Button>("ConfirmLeaveSoloButton").clicked += () =>
            {
                Resume();
                NetworkRequests.ReturnToMainMenu?.Invoke();
            };
            modal.EnableInClassList("is-visible", pause.IsPaused);
        }

        public void Pause()
        {
            if (!inGame || !SoloSession.Active) return;
            var worlds = new List<World>();
            foreach (var world in World.All)
                if ((world.Flags & (WorldFlags.GameClient | WorldFlags.GameServer | WorldFlags.Game)) != 0) worlds.Add(world);
            pause.Pause(worlds);
            modal?.AddToClassList("is-visible");
            root?.Q<Button>("ResumeSoloButton")?.Focus();
        }

        public void Resume()
        {
            pause.Resume();
            modal?.RemoveFromClassList("is-visible");
            exitConfirmation?.RemoveFromClassList("is-visible");
        }

        void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        void OnDisable() => Resume();
        void OnDestroy()
        {
            pause.Dispose();
            if (overlay != null) Destroy(overlay);
            if (settings != null) Destroy(settings);
            if (instance == this) instance = null;
        }
    }
}
