using System.Collections.Generic;
using System.Linq;
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
    private readonly Dictionary<CharacterButtonState, FixedString64Bytes> _buttonStateToClassName; 
    private readonly ButtonsStateTracker _buttonStateTracker;
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

        _buttonStateTracker = new ButtonsStateTracker();
        _buttonStateTracker.RegisterButton(AvocadoName);
        _buttonStateTracker.RegisterButton(BirdName);
        _buttonStateTracker.RegisterButton(CoinName);
        _buttonStateTracker.RegisterButton(LiraName);
        _buttonStateTracker.RegisterButton(MugName);
        _buttonStateTracker.RegisterButton(TucTucName);

        _buttonDefaultColor = Screen.AvocadoButton.style.backgroundColor;
        _buttonStateToClassName = new ()
        {
            { CharacterButtonState.Default, "char-not-picked-btn-container" },
            { CharacterButtonState.Unavailable, "char-disabled-btn-container" },
            { CharacterButtonState.Choosing, "char-picked-btn-container" },
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
        var characterButton = Context.CharacterButton;
        _buttonStateTracker.ResetAvaiblabeButtonsState();
        _buttonStateTracker.UpdateState(characterButton.Name, characterButton.State);
        ResetButtonContext();
        PaintButtons();
    }

    private void ResetButtonContext()
    {
        var tempContext = Context;
        tempContext.CharacterButton.Name = default;
        tempContext.CharacterButton.State = default;
        Context = tempContext;
    }

    private void PaintButtons()
    {
        for (int i = 0; i < _buttonRegistry.Count; i++)
        {
            var buttonName = _buttonRegistry.Keys.ElementAt(i);
            _buttonStateTracker.TryGetState(buttonName, out var buttonState);
            switch (buttonState)
            {
                case CharacterButtonState.Default:
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Unavailable].ToString());
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Choosing].ToString());
                    _buttonRegistry[buttonName].parent.AddToClassList(_buttonStateToClassName[CharacterButtonState.Default].ToString());
                    break;
                case CharacterButtonState.Unavailable:
                    _buttonRegistry[buttonName].parent.AddToClassList(_buttonStateToClassName[CharacterButtonState.Unavailable].ToString());
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Choosing].ToString());
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Default].ToString());
                    break;
                case CharacterButtonState.Choosing:
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Unavailable].ToString());
                    _buttonRegistry[buttonName].parent.AddToClassList(_buttonStateToClassName[CharacterButtonState.Choosing].ToString());
                    _buttonRegistry[buttonName].parent.RemoveFromClassList(_buttonStateToClassName[CharacterButtonState.Default].ToString());
                    break;
            }
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
        {
            _buttonStateTracker.TryGetState(name, out var buttonState);
            _dataEventBufferQuery.GetSingletonBuffer<CharacterSelectedEventBuffer>()
                .Add(new CharacterSelectedEventBuffer 
                { 
                    CharacterButtonSelected = new CharacterButton{ Name = name, State = buttonState }
                });
        }
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
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }
}
