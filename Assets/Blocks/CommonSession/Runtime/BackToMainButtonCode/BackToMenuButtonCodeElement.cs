using System.Collections.Generic;
using Assets.Common;
using Assets.UI.Runtime;
using Blocks.Sessions.Common.Assets.Blocks.CommonSession.Runtime.BackToMainButtonCode;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class BackToMenuButtonCodeElement : VisualElement
    {
        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value)
                    return;

                m_SessionType = value;
                UnityEngine.Debug.Log($"[CopySessionCodeElement] | setting session type: {value}");
                if (panel != null)
                    UpdateBindings();
            }
        }
        string m_SessionType;

        readonly List<DataBinding> m_Bindings = new();

        BackToMenuButtonViewModel m_ViewModel;

        public BackToMenuButtonCodeElement()
        {
            var backToMenuButton = new Button
            {
                text = "<-"
            };
            AddToClassList(NetworkMenuTheme.BlueButton);
            backToMenuButton.clicked += HandleBackButton;

            var hasSessionCode = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(BackToMenuButtonViewModel.HasNoSessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            backToMenuButton.SetBinding(new BindingId(nameof(enabledSelf)), hasSessionCode);

            Add(backToMenuButton);
            m_Bindings.Add(hasSessionCode);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();

            UnityEngine.Debug.Log($"[BackButtonPanel] | update bindings");
            m_ViewModel = new BackToMenuButtonViewModel(SessionType);
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
            NetworkRequests.GoBackToMainMenu = true;
            UnityEngine.Debug.Log($"[BackToMenuButtonCodeElement] | BackButtonClicked");
        }
    }
}
