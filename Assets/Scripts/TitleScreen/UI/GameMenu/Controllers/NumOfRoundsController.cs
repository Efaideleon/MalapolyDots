using Unity.Entities;

public class NumOfRoundsController
{
    public NumOfRoundsScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;

    public NumOfRoundsController(NumOfRoundsScreen screen)
    {
        Screen = screen;
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void SubscribeEvents()
    {
        Screen.Button8Rounds.clickable.clicked += OnButton8Clicked;
        Screen.Button12Rounds.clickable.clicked += OnButton12Clicked;
        Screen.Button16Rounds.clickable.clicked += OnButton16Clicked;
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void OnButton8Clicked() => DispatchDataEvent(8);
    private void OnButton12Clicked() => DispatchDataEvent(12);
    private void OnButton16Clicked() => DispatchDataEvent(16);

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfRounds });
    }

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfRoundsEventBuffer>()
                .Add(new NumberOfRoundsEventBuffer { NumberOfRounds = num });
        else
            UnityEngine.Debug.LogWarning("_eventBufferQuery not set in NumOfPlayersController");
    }

    public void OnDispose()
    {
        Screen.Button8Rounds.clickable.clicked -= OnButton8Clicked;
        Screen.Button12Rounds.clickable.clicked -= OnButton12Clicked;
        Screen.Button16Rounds.clickable.clicked -= OnButton16Clicked;
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
