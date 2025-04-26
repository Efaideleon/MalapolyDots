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
    public AvailableState State;
}

public enum ButtonState
{
    Default,
    Choosing,
    Unavailable
}

public class CharacterSelectionController
{
    private static readonly Dictionary<ButtonState, string> _stateClasses = new ()
    {
        { ButtonState.Default, "char-not-picked-btn-container" },
        { ButtonState.Unavailable, "char-disabled-btn-container" },
        { ButtonState.Choosing, "char-picked-btn-container" },
    }; 
    private readonly Dictionary<Character, Button> _buttonRegistry = new();
    private readonly Dictionary<Character, Action> _dispatchEventHandlers = new();
    private readonly Dictionary<Character, Action> _colorEventHandlers = new();
    private readonly ButtonsStateTracker _buttonStateTracker = new();
    public CharacterSelectionContext Context { get; set; }
    public CharacterSelectionScreen Screen { get; private set;}
    private EntityQuery _dataEventBufferQuery; 
    private EntityQuery _changeScreenQuery;
    private Character _previousCharacterClicked = Character.None;

    public CharacterSelectionController(CharacterSelectionScreen screen, CharacterSelectionContext context)
    {
        Screen = screen;
        Context = context;

        for (int i = 0; i < Screen.CharButtons.Length; i++)
            _buttonRegistry.Add(CharacterData.AllCharacters[i], Screen.CharButtons[i]);

        foreach (var buttonType in CharacterData.AllCharacters)
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
        SetButtonUnavailable(characterButton.Type, characterButton.State);
        ResetButtonContext();
    }

    private void SetButtonUnavailable(Character type, AvailableState state)
    {
        if (state == AvailableState.Unavailable)
        {
            _buttonStateTracker.UpdateState(type, state);
            SetButtonColor(type, ButtonState.Unavailable);
        }
    }

    public void SetButtonColor(Character type, ButtonState state)
    {
        foreach (ButtonState s in Enum.GetValues(typeof(ButtonState)))
            _buttonRegistry[type].parent.EnableInClassList(_stateClasses[s], s == state);
    }

    private void ResetButtonContext()
    {
        var tempContext = Context;
        tempContext.CharacterButton.Type = Character.None;
        tempContext.CharacterButton.State = default;
        Context = tempContext;
    }

    public void SubscribeEvents()
    {
        foreach (var kvp in _buttonRegistry)
        {
            var type = kvp.Key;
            var button = kvp.Value;

            void handle() => DispatchDataEvent(type);
            _dispatchEventHandlers.Add(type, handle);
            button.clickable.clicked += handle;

            void colorHandle() => UpdateButtonClass(type);
            _colorEventHandlers.Add(type, colorHandle);
            button.clickable.clicked += colorHandle;
        }
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void UpdateButtonClass(Character buttonType)
    {
        if (_previousCharacterClicked != Character.None)
            if (_buttonStateTracker.TryGetState(_previousCharacterClicked, out var state))
                if (state == AvailableState.Available)
                    SetButtonColor(_previousCharacterClicked, ButtonState.Default);

        if (_buttonStateTracker.TryGetState(buttonType, out var currentState))
            if (currentState == AvailableState.Available)
                SetButtonColor(buttonType, ButtonState.Choosing);

        _previousCharacterClicked = buttonType;
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
        foreach (var kvp in _dispatchEventHandlers)
        {
            var type = kvp.Key;
            var handler = kvp.Value;
            _buttonRegistry[type].clickable.clicked -= handler;
        }

        foreach (var kvp in _colorEventHandlers)
        {
            var type = kvp.Key;
            var handler = kvp.Value;
            _buttonRegistry[type].clickable.clicked -= handler;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
