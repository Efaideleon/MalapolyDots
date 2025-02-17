using UnityEngine.UIElements;

public class TitleScreen
{
    private VisualElement _root;
    public Button StartButton { get; private set; }

    public TitleScreen(VisualElement root)
    {
        _root = root.Q<VisualElement>("TitleScreen");
        StartButton = _root.Q<Button>("start-button");
    }
}