using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.Common;
using Assets.Common.Assets.Common;
using Assets.Scripts.TitleScreen.NetworkUI.Panels;
using TitleScreen.NetworkUI.Authoring;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnCreateGameSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CreateGameUIReference>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var uiRef = SystemAPI.ManagedAPI.GetSingleton<CreateGameUIReference>();
            var prefab = uiRef.uiDocumentGO;

            if (prefab == null)
                return;

            Debug.Log("[SpawnCreateGameSystem] Spawning CreateGame UI Document");
            var uiGameObject = UnityEngine.Object.Instantiate(prefab);
            if (!uiGameObject.TryGetComponent<UIDocument>(out var uiDocument))
            {
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }

            var uiRequests = SystemAPI.ManagedAPI.GetSingleton<GameMenuUIRequests>();
            var root = uiDocument.rootVisualElement;
            var createGamePanel = new CreateGamePanel(root, uiRequests.Queue);

            // Create managed singleton component holding the panel instance.
            state.EntityManager.CreateSingleton(new CreateGameUIPanelComponent { Panel = createGamePanel });
        }

        public void OnStopRunning(ref SystemState state)
        {
            // nothing special on stop for now
        }

        public void OnDestroy(ref SystemState state)
        {
            var createGameComponent = SystemAPI.ManagedAPI.GetSingleton<CreateGameUIPanelComponent>();
            createGameComponent.Panel.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideCreateGamePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CreateGameUIPanelComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var menuPhase = SystemAPI.GetSingleton<GameMenuPhaseComponent>();
            var panel = SystemAPI.ManagedAPI.GetSingleton<CreateGameUIPanelComponent>().Panel;
            if (panel == null)
                return;

            switch (menuPhase.Value)
            {
                // Adjust these cases to match the project's GameMenuPhase values.
                //case GameMenuPhase.HostSetup:
                case GameMenuPhase.HostSetup: // if you have a CreateGame phase
                    if (!panel.IsVisible) panel.Show();
                    break;

                default:
                    if (panel.IsVisible) panel.Hide();
                    break;
            }
        }
    }

    // Managed component used as a singleton container for the CreateGame panel.
    public class CreateGameUIPanelComponent : IComponentData
    {
        public CreateGamePanel Panel;
    }

    // Simple wrapper around the UI Document root for the Create Game UI.
    public class CreateGamePanel : System.IDisposable
    {
        private readonly VisualElement _root;

        public bool IsVisible => _root.style.display != DisplayStyle.None;
        private readonly Button _backButton;
        private readonly Queue<UIRequest> uiRequests;

        readonly List<DataBinding> m_Bindings = new();

        BackButtonCreateGameViewModel m_ViewModel;

        public CreateGamePanel(VisualElement root, Queue<UIRequest> uiRequests)
        {
            _root = root;
            this.uiRequests = uiRequests;
            _backButton = _root.Q<Button>("BackButton");
            Subscribe();
            Hide();
        }

        private void Subscribe()
        {
            var hasSessionCode = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(BackButtonCreateGameViewModel.HasSessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            _backButton.SetBinding(new BindingId(nameof(_backButton.enabledSelf)), hasSessionCode);

            m_Bindings.Add(hasSessionCode);

            // _backButton.RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            // _backButton.RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());

            UpdateBindings();
            _backButton.clickable.clicked += HandleBackButton;
        }

        public void Dispose()
        {
            CleanupBindings();
            _backButton.clickable.clicked -= HandleBackButton;
        }

        void UpdateBindings()
        {
            CleanupBindings();

            UnityEngine.Debug.Log($"[BackButtonPanel] | update bindings");
            m_ViewModel = new BackButtonCreateGameViewModel("sessionType");
            foreach (var binding in m_Bindings)
            {
                binding.dataSource = m_ViewModel;
            }
        }

        void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;

            foreach (var binding in m_Bindings)
            {
                binding.dataSource = null;
            }
        }

        private void HandleBackButton()
        {
            UnityEngine.Debug.Log($"[SpawnCreateGameSystem] | backButton clicked");
            this.uiRequests.Enqueue(new UIRequest { Value = UIRequestType.BackToMainMenu });
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }

    public class BackButtonCreateGameViewModel : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        const string k_NoSessionText = "No Session Code";

        long _updateVersion;
        SessionObserver m_SessionObserver;
        ISession m_Session;

        [CreateProperty]
        public bool HasSessionCode
        {
            get => m_HasSessionCode;
            private set
            {
                if (m_HasSessionCode == value)
                    return;

                m_HasSessionCode = value;
                Notify();
            }
        }
        bool m_HasSessionCode;

        [CreateProperty]
        public string SessionCode
        {
            get => m_SessionCode;
            private set
            {
                if (m_SessionCode == value)
                    return;

                m_SessionCode = value;
                HasSessionCode = m_SessionCode != k_NoSessionText;
                ++_updateVersion;

                Notify();
            }
        }
        string m_SessionCode = k_NoSessionText;

        public BackButtonCreateGameViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);

            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            SessionCode = session?.Code;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
        }

        void OnSessionRemoved()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            SessionCode = k_NoSessionText;
            m_Session = null;
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
                m_SessionObserver.SessionAdded -= OnSessionAdded;
                m_SessionObserver.Dispose();
                m_SessionObserver = null;
            }
            if (m_Session != null)
            {
                m_Session.RemovedFromSession -= OnSessionRemoved;
                m_Session.Deleted -= OnSessionRemoved;
                m_Session = null;
            }
        }


        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        void Notify([CallerMemberName] string property = null) =>
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));

        public long GetViewHashCode() => _updateVersion;
    }
}
