#nullable enable
using System.Collections.Generic;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public abstract class NetworkPanelBase
    {
        protected VisualElement Root;
        protected Queue<UIRequest> UIRequests;

        protected NetworkPanelBase(VisualElement root, Queue<UIRequest> uIRequests)
        {
            Root = root;
            UIRequests = uIRequests;
        }

        public void Show()
        {
            Root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            Root.style.display = DisplayStyle.None;
        }

        public abstract void Initialize(); 
        public abstract void Dispose();
    }
}

