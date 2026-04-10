using Blocks.Common;
using UnityEngine.UIElements;

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

            startGameButton.AddToClassList(BlocksTheme.Button);
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
            // TODO: make sure that we are in a session.
        }
    }
}
