using Assets.Common;
using Assets.Common.Assets.Common;
using Blocks.Common;
using UnityEngine.UIElements;
using Assets.UI.Runtime;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class StartGameCodeElement : VisualElement
    {
        const string StartGameButtonText = "Start";

        public StartGameCodeElement()
        {
            var startGameButton = new Button
            {
                text = StartGameButtonText
            };

            startGameButton.AddToClassList(NetworkMenuTheme.BlueButton);
            startGameButton.clicked += StartGame;

            Add(startGameButton);
            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        private void UpdateBindings()
        {
            UnityEngine.Debug.Log($"[StartGameCodeElement] | attaching to the panel");
        }

        private void CleanupBindings()
        {
            UnityEngine.Debug.Log($"[StartGameCodeElement] | detaching from the panel");
        }

        private void StartGame()
        {
            UnityEngine.Debug.Log($"[StartGameCodeElement] | Starting game...");
            NetworkRequests.StartGame = true;
        }
    }
}
