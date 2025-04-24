using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public struct CharacterSelectionContext
{
    public int PlayerNumber;
    public CharacterButton CharacterButton;
}

public struct CharacterButton
{
    public FixedString64Bytes Name;
    public CharacterButtonState State;
}

public enum CharacterButtonState
{
    Default,
    Choosing,
    Unavailable
}

public class CharacterSelectionControler
{
    private const string AvocadoName = "Avocado";
    private const string BirdName = "Bird";
    private const string CoinName = "Coin";
    private const string LiraName = "Lira";
    private const string MugName = "Mug";
    private const string TucTucName = "TucTuc";
    private readonly Dictionary<FixedString64Bytes, Button> _buttonRegistry; 
    private readonly Dictionary<FixedString64Bytes, CharacterButtonState> _buttonStateTracker; 
    public CharacterSelectionContext Context { get; set; }
    public CharacterSelectionScreen Screen { get; private set;}
    private EntityQuery _dataEventBufferQuery; 
    private EntityQuery _changeScreenQuery;
    private StyleColor _buttonDefaultColor = default;


    public CharacterSelectionControler(CharacterSelectionScreen screen, CharacterSelectionContext context)
    {
        Screen = screen;
        Context = context;
        _buttonRegistry = new () 
        {
            { AvocadoName, Screen.AvocadoButton },
            { BirdName, Screen.BirdButton },
            { CoinName, Screen.CoinButton },
            { LiraName, Screen.LiraButton },
            { MugName, Screen.MugButton },
            { TucTucName, Screen.TuctucButton }
        };
        _buttonStateTracker = new ()
        {
            { AvocadoName, CharacterButtonState.Default },
            { BirdName, CharacterButtonState.Default },
            { CoinName, CharacterButtonState.Default },
            { LiraName, CharacterButtonState.Default },
            { MugName, CharacterButtonState.Default },
            { TucTucName, CharacterButtonState.Default },
        };
        _buttonDefaultColor = Screen.AvocadoButton.style.backgroundColor;
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventBufferQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    public void Update() 
    {
        Screen.PlayerNumberLabel.text = Context.PlayerNumber.ToString();
        var characterButton = Context.CharacterButton;
        _buttonRegistry.TryGetValue(characterButton.Name, out var button);
        if (button != null)
        {
            foreach (var b in _buttonRegistry)
            {
                if (_buttonStateTracker[b.Key] != CharacterButtonState.Unavailable)
                {
                    _buttonStateTracker[b.Key] = CharacterButtonState.Default;
                }
            }
            foreach (var b in _buttonStateTracker)
            {
                if (b.Value == CharacterButtonState.Default)
                {
                    var label = _buttonRegistry[b.Key].Q<Label>("btn-label");
                    if (label != null)
                    {
                        _buttonRegistry[b.Key].Q<Label>("btn-label").style.backgroundColor = new StyleColor(Color.yellow);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"Label is null");
                    }
                }
            }
            switch (characterButton.State)
            {
                case CharacterButtonState.Choosing:
                    UnityEngine.Debug.Log($"changing: {button.name} to red");
                    button.Q<Label>("btn-label").style.backgroundColor = new StyleColor(Color.red);
                    _buttonStateTracker[characterButton.Name] = characterButton.State; 
                    break;
                case CharacterButtonState.Unavailable:
                    UnityEngine.Debug.Log($"changing: {button.name} to blue");
                    button.Q<Label>("btn-label").style.backgroundColor = new StyleColor(Color.blue);
                    _buttonStateTracker[characterButton.Name] = characterButton.State; 
                    break;

            }
            var tempContext = Context;
            tempContext.CharacterButton.Name = default;
            tempContext.CharacterButton.State = default;
            Context = tempContext;
        }
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
                .Add(new CharacterSelectedEventBuffer 
                { 
                    CharacterButtonSelected = new CharacterButton{ Name = name, State = _buttonStateTracker[name] }
                });
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
