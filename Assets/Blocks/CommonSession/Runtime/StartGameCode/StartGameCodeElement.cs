using Assets.Common;
using Assets.Common.Assets.Common;
using Blocks.Common;
using UnityEngine.UIElements;
using Assets.UI.Runtime;
using System.Collections.Generic;
using Blocks.Sessions.Common.Assets.Blocks.CommonSession.Runtime.StartGameCode;
using Unity.Properties;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class StartGameCodeElement : VisualElement
    {
        const string StartGameButtonText = "Start";

        readonly List<DataBinding> m_Bindings = new();

        StartGameCodeViewModel m_ViewModel;

        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value)
                    return;

                m_SessionType = value;
                UnityEngine.Debug.Log($"[StartGameCodeElement] | SessionType: {value}");
                if (panel != null)
                    UpdateBindings();
            }
        }
        string m_SessionType;

        public StartGameCodeElement()
        {
            var hasSessionCode = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(StartGameCodeViewModel.HasSessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(enabledSelf)), hasSessionCode);

            var startGameButton = new Button
            {
                text = StartGameButtonText
            };
            startGameButton.AddToClassList(NetworkMenuTheme.BlueButton);
            startGameButton.clicked += StartGame;
            Add(startGameButton);
            m_Bindings.Add(hasSessionCode);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        private void UpdateBindings()
        {
            CleanupBindings();

            UnityEngine.Debug.Log($"[StartGameCodeElement] | SessionType: {SessionType}");
            m_ViewModel = new StartGameCodeViewModel(SessionType);
            foreach (var binding in m_Bindings)
            {
                binding.dataSource = m_ViewModel;
            }
            UnityEngine.Debug.Log($"[StartGameCodeElement] | attaching to the panel");
        }

        private void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;

            foreach (var binding in m_Bindings)
            {
                binding.dataSource = null;
            }
            UnityEngine.Debug.Log($"[StartGameCodeElement] | detaching from the panel");
        }

        private void StartGame()
        {
            UnityEngine.Debug.Log($"[StartGameCodeElement] | Starting game...");
            NetworkRequests.StartGame = true;
        }
    }
}
