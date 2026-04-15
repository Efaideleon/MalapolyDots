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
                if (panel != null)
                    UpdateBindings();
            }
        }
        string m_SessionType;

        public StartGameCodeElement()
        {
            var startGameButton = new Button
            {
                text = StartGameButtonText
            };

            startGameButton.AddToClassList(NetworkMenuTheme.BlueButton);
            startGameButton.SetEnabled(false);
            startGameButton.clicked += StartGame;

            Add(startGameButton);

            var sessionCodeBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(CopySessionCodeViewModel.SessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            m_Bindings.Add(sessionCodeBinding);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        private void UpdateBindings()
        {
            CleanupBindings();

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
