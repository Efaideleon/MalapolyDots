using Unity.Entities;

public struct CharacterSelectionContext
{
    public int PlayerNumber;
}

public class CharacterSelectionControler
{
    private const string AvocadoName = "Avocado";
    private const string BirdName = "Bird";
    private const string CoinName = "Coin";
    private const string LiraName = "Lira";
    private const string MugName = "Mug";
    private const string TucTucName = "TucTuc";
    public CharacterSelectionContext Context { get; set; }
    public CharacterSelectionScreen Screen { get; private set;}
    private EntityQuery _dataEventBufferQuery; 
    private EntityQuery _changeScreenQuery;

    public CharacterSelectionControler(CharacterSelectionScreen screen, CharacterSelectionContext context)
    {
        Screen = screen;
        Context = context;
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventBufferQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    public void Update() 
    {
        //TODO: Update the text label for the current player basedon the context
    }

    public void SubscribeEvents()
    {
        Screen.AvocadoButton.clickable.clicked += HandleAvocadoButton;
        Screen.BirdButton.clickable.clicked += HandleBirdButton;
        Screen.CoinButton.clickable.clicked += HandleCoinButton;
        Screen.LiraButton.clickable.clicked += HandleLiraButton;
        Screen.MugButton.clickable.clicked += HandleMugButton;
        Screen.TuctucButton.clickable.clicked += HandleTuctucButton;
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void HandleAvocadoButton() => DispatchDataEvent(AvocadoName);
    private void HandleBirdButton() => DispatchDataEvent(BirdName);
    private void HandleCoinButton() => DispatchDataEvent(CoinName);
    private void HandleLiraButton() => DispatchDataEvent(LiraName);
    private void HandleMugButton() => DispatchDataEvent(MugName);
    private void HandleTuctucButton() => DispatchDataEvent(TucTucName);

    private void DispatchDataEvent(string name)
    {
        if (_dataEventBufferQuery != null)
            _dataEventBufferQuery.GetSingletonBuffer<CharacterSelectedEventBuffer>()
                .Add(new CharacterSelectedEventBuffer { Name = name });
        else
            UnityEngine.Debug.LogWarning("_eventBufferQuery not set in CharacterSelectionControler");
    }

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.CharacterSelection });
    }

    public void OnDispose()
    {
        Screen.AvocadoButton.clickable.clicked -= HandleAvocadoButton;
        Screen.BirdButton.clickable.clicked -= HandleBirdButton;
        Screen.CoinButton.clickable.clicked -= HandleCoinButton;
        Screen.LiraButton.clickable.clicked -= HandleLiraButton;
        Screen.MugButton.clickable.clicked -= HandleMugButton;
        Screen.TuctucButton.clickable.clicked -= HandleTuctucButton;
    }
}
