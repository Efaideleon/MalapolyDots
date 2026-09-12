using Assets.UI.Runtime;
using Blocks.Sessions.Common.Assets.Blocks.CommonSession.Runtime.StartGameCode;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class StartGameCodeElement : VisualElement
    {
        readonly Button m_Start;
        readonly Label m_Status;
        StartGameCodeViewModel m_ViewModel;
        string m_SessionType;

        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value) return;
                m_SessionType = value;
                if (panel != null) Bind();
            }
        }

        public StartGameCodeElement()
        {
            m_Status = new Label { enableRichText = false };
            m_Status.style.whiteSpace = WhiteSpace.Normal;
            Add(m_Status);
            m_Start = new Button(() => { if (m_ViewModel != null) _ = m_ViewModel.StartAsync(); }) { text = "Start character selection" };
            m_Start.AddToClassList(NetworkMenuTheme.BlueButton);
            m_Start.style.width = Length.Percent(100);
            m_Start.style.maxWidth = Length.Percent(100);
            m_Start.style.height = StyleKeyword.Auto;
            m_Start.style.minHeight = 64;
            m_Start.style.flexShrink = 0;
            m_Start.style.fontSize = 22;
            m_Start.style.whiteSpace = WhiteSpace.Normal;
            m_Start.style.paddingLeft = 12;
            m_Start.style.paddingRight = 12;
            m_Start.style.paddingTop = 12;
            m_Start.style.paddingBottom = 12;
            Add(m_Start);
            RegisterCallback<AttachToPanelEvent>(_ => Bind());
            RegisterCallback<DetachFromPanelEvent>(_ => Cleanup());
        }

        void Bind()
        {
            Cleanup();
            m_ViewModel = new StartGameCodeViewModel(SessionType);
            m_ViewModel.Changed += Render;
            Render();
        }

        void Render()
        {
            m_Start.SetEnabled(m_ViewModel.CanStart);
            m_Status.text = m_ViewModel.Status;
        }

        void Cleanup()
        {
            if (m_ViewModel == null) return;
            m_ViewModel.Changed -= Render;
            m_ViewModel.Dispose();
            m_ViewModel = null;
        }
    }
}
