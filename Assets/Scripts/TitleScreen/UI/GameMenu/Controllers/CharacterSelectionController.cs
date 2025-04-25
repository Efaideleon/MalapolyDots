using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.UIElements;

public struct CharacterSelectionContext
{
    public int PlayerNumber;
    public CharacterButton CharacterButton;
}

public struct CharacterButton
{
    public Character Type;
    public ButtonState State;
}

public enum ButtonState
{
    Default,
    Choosing,
    Unavailable
}

public enum Character
{
    None,
    Avocado,
    Bird,
    Coin,
    Lira,
    Mug,
    TucTuc
}

public class CharacterSelectionController
{
    private static readonly Character[] allCharButtonTypes = new []
    {
        Character.Avocado,
        Character.Bird,
        Character.Coin,
        Character.Lira,
        Character.Mug,
        Character.TucTuc,
    };
    private static readonly Dictionary<ButtonState, string> _stateClasses = new ()
    {
        { ButtonState.Default, "char-not-picked-btn-container" },
        { ButtonState.Unavailable, "char-disabled-btn-container" },
        { ButtonState.Choosing, "char-picked-btn-container" },
    }; 
    private readonly Dictionary<Character, Button> _buttonRegistry = new();
    private readonly Dictionary<Character, Action> _buttonHandlers = new();
    private readonly ButtonsStateTracker _buttonStateTracker = new();
    public CharacterSelectionContext Context { get; set; }
    public CharacterSelectionScreen Screen { get; private set;}
    private EntityQuery _dataEventBufferQuery; 
    private EntityQuery _changeScreenQuery;

    public CharacterSelectionController(CharacterSelectionScreen screen, CharacterSelectionContext context)
    {
        Screen = screen;
        Context = context;

        for (int i = 0; i < Screen.CharButtons.Length; i++)
            _buttonRegistry.Add(allCharButtonTypes[i], Screen.CharButtons[i]);

        foreach (var buttonType in allCharButtonTypes)
            _buttonStateTracker.RegisterButton(buttonType);

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
        _buttonStateTracker.UpdateState(characterButton.Type, characterButton.State);
        ResetButtonContext();
        PaintButtons();
    }

    private void ResetButtonContext()
    {
        var tempContext = Context;
        tempContext.CharacterButton.Type = Character.None;
        tempContext.CharacterButton.State = default;
        Context = tempContext;
    }

    private void PaintButtons()
    {
        foreach(var (buttonType, _) in _buttonRegistry)
        {
            _buttonStateTracker.TryGetState(buttonType, out var buttonState);
            foreach (var (state, className) in _stateClasses)
                _buttonRegistry[buttonType].parent.EnableInClassList(className, state == buttonState);
        }
    }

    public void SubscribeEvents()
    {
        foreach (var kvp in _buttonRegistry)
        {
            var type = kvp.Key;
            var button = kvp.Value;

            void handle() => DispatchDataEvent(type);
            _buttonHandlers.Add(type, handle);
            button.clickable.clicked += handle;
        }
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void DispatchDataEvent(Character buttonType)
    {
        if (_dataEventBufferQuery != null)
        {
            _buttonStateTracker.TryGetState(buttonType, out var buttonState);
            _dataEventBufferQuery.GetSingletonBuffer<CharacterSelectedEventBuffer>()
                .Add(new CharacterSelectedEventBuffer 
                { 
                    CharacterButtonSelected = new CharacterButton{ Type = buttonType, State = buttonState }
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
        foreach (var kvp in _buttonHandlers)
        {
            var type = kvp.Key;
            var handler = kvp.Value;
            _buttonRegistry[type].clickable.clicked -= handler;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
