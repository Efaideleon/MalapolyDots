using Unity.Entities;

public class NumOfRoundsController
{
    public IOptionsScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private readonly OptionsController _optionsController;

    public NumOfRoundsController(NumOfRoundsScreen screen, EntityQuery changeScreenQuery, EntityQuery dataEventQuery)
    {
        Screen = screen;
        _changeScreenQuery = changeScreenQuery;
        _dataEventQuery = dataEventQuery;
        _optionsController = new(Screen, DispatchDataEvent, _changeScreenQuery, ScreenType.NumOfRounds);
    }

    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfRoundsEventBuffer>()
                .Add(new NumberOfRoundsEventBuffer { NumberOfRounds = num });
        else
            UnityEngine.Debug.LogWarning("[NumOfRoundsController] | _eventBufferQuery not set in NumOfPlayersController");
    }

    public void OnDispose() 
    {
        _optionsController.OnDispose();
    }
}
