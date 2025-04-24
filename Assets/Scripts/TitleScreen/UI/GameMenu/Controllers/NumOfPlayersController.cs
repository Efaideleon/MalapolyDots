using UI.GameMenu;
using Unity.Entities;

//TODO: we could use an update loop with context to change the look of the panel
public class NumOfPlayersController
{
    public NumberOfPlayersScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery; 
    private EntityQuery _changeScreenQuery; 

    public NumOfPlayersController(NumberOfPlayersScreen screen)
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
        Screen.Button2Players.clickable.clicked += OnButton2Clicked;
        Screen.Button3Players.clickable.clicked += OnButton3Clicked;
        Screen.Button4Players.clickable.clicked += OnButton4Clicked;
        Screen.Button5Players.clickable.clicked += OnButton5Clicked;
        Screen.Button6Players.clickable.clicked += OnButton6Clicked;
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void OnButton2Clicked() => DispatchDataEvent(2);
    private void OnButton3Clicked() => DispatchDataEvent(3);
    private void OnButton4Clicked() => DispatchDataEvent(4);
    private void OnButton5Clicked() => DispatchDataEvent(5);
    private void OnButton6Clicked() => DispatchDataEvent(6);

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfPlayersEventBuffer>()
                .Add(new NumberOfPlayersEventBuffer { NumberOfPlayers = num });
        else
            UnityEngine.Debug.LogWarning("_eventBufferQuery not set in NumOfPlayersController");
    }

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfPlayers });
    }

    public void OnDispose()
    {
        Screen.Button2Players.clickable.clicked -= OnButton2Clicked;
        Screen.Button3Players.clickable.clicked -= OnButton3Clicked;
        Screen.Button4Players.clickable.clicked -= OnButton4Clicked;
        Screen.Button5Players.clickable.clicked -= OnButton5Clicked;
        Screen.Button6Players.clickable.clicked -= OnButton6Clicked;
    }
}
