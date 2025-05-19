using Unity.Entities;

public class TitleScreenController
{
    public TitleScreen Screen {get; private set; }
    private EntityQuery _changeScreenQuery;

    public TitleScreenController(TitleScreen screen)
    {
        Screen = screen;
        SubscribeEvents();
    }

    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void SubscribeEvents()
    {
        Screen.StartButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.Title });
    }

    public void Dispose()
    {
        Screen.StartButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
