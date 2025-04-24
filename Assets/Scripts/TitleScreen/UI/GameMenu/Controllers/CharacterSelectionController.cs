using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public struct CharacterSelectionContext
{
    public int PlayerNumber;
    public Queue<CharacterButton> CharacterButtonEventQueue;
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
    public CharacterSelectionContext Context { get; set; }
    public CharacterSelectionScreen Screen { get; private set;}
    private EntityQuery _dataEventBufferQuery; 
    private EntityQuery _changeScreenQuery;



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
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventBufferQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    public void Update() 
    {
        Screen.PlayerNumberLabel.text = Context.PlayerNumber.ToString();
        while (Context.CharacterButtonEventQueue.Count > 0) 
        {
            var e = Context.CharacterButtonEventQueue.Dequeue();
            var label = _buttonRegistry[e.Name].Q<Label>("Label");
            label.style.color = new StyleColor(Color.red);
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
                    CharacterButtonSelected = new CharacterButton{ Name = name, State = CharacterButtonState.Default }
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
