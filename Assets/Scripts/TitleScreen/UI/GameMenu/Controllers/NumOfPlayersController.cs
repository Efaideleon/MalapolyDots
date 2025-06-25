using UI.GameMenu;
using Unity.Entities;

public class NumOfPlayersController
{
    public IOptionsScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private readonly OptionsController _optionsController;

    public NumOfPlayersController(NumberOfPlayersScreen screen, EntityQuery changeScreenQuery, EntityQuery dataEventQuery) 
    {
        Screen = screen;
        _dataEventQuery = dataEventQuery;
        _changeScreenQuery = changeScreenQuery;
        _optionsController = new(Screen, DispatchDataEvent, _changeScreenQuery, ScreenType.NumOfPlayers);
    }

    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfPlayersEventBuffer>()
                .Add(new NumberOfPlayersEventBuffer { NumberOfPlayers = num });
        else
            UnityEngine.Debug.LogWarning("[NumOfPlayersController] | _eventBufferQuery not set in NumOfPlayersController");
    }

    public void OnDispose() => _optionsController.OnDispose();
}
