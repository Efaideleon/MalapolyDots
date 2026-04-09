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

            Add(startGameButton);
        }
    }
}
