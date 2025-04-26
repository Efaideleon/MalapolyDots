using System;
using System.Collections.Generic;
using UI.GameMenu;
using Unity.Entities;
using UnityEngine.UIElements;

public enum ButtonPlayerType
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
}

public class ButtonPlayer
{
    public VisualElement Parent { get; set; }
    public Button Button { get; set; }
    public readonly int Value;
    public Action OnClick;
    public Action OnBorderChange;

    public ButtonPlayer(VisualElement parent, Button button, int value, Action onClick, Action onBorderChange)
    {
        Parent = parent;
        Button = button;
        Value = value;
        OnClick = onClick;
        OnBorderChange = onBorderChange;
    }
}

public class NumOfPlayersController
{
    private readonly ButtonPlayerType[] AllButtonPlayerTypes = 
    {
        ButtonPlayerType.Two,
        ButtonPlayerType.Three,
        ButtonPlayerType.Four,
        ButtonPlayerType.Five,
        ButtonPlayerType.Six,
    };
    public NumberOfPlayersScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery; 
    private EntityQuery _changeScreenQuery; 
    private VisualElement _previousButtonContainer = null;
    public const string BorderClassName = "dbr-btn-picked";
    private readonly Dictionary<ButtonPlayerType, ButtonPlayer> ButtonRegistry = new();

    public NumOfPlayersController(NumberOfPlayersScreen screen)
    {
        Screen = screen;
        for (int i = 0; i < Enum.GetValues(typeof(ButtonPlayerType)).Length; i++)
        {
            var buttonValue = i + 2;
            void OnClick() => DispatchDataEvent(buttonValue);
            var button = Screen.Buttons[i];
            var container = Screen.ButtonsContainer[i];
            void OnBorderChange() => UpdateBorder(container);
            ButtonRegistry.Add(AllButtonPlayerTypes[i], new ButtonPlayer
            (
                container, button, buttonValue, OnClick, OnBorderChange
            ));
        }
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void SubscribeEvents()
    {
        foreach (var kvp in ButtonRegistry)
        {
            var button = kvp.Value.Button;
            var onClick = kvp.Value.OnClick;
            var onBorderChange = kvp.Value.OnBorderChange;
            kvp.Value.Button.clickable.clicked += onClick;
            kvp.Value.Button.clickable.clicked += onBorderChange;
        }
        Screen.ConfirmButton.clickable.clicked += DispatchScreenChangeEvent;
    }

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfPlayersEventBuffer>()
                .Add(new NumberOfPlayersEventBuffer { NumberOfPlayers = num });
        else
            UnityEngine.Debug.LogWarning("_eventBufferQuery not set in NumOfPlayersController");
    }

    private void UpdateBorder(VisualElement container)
    {
        _previousButtonContainer?.EnableInClassList(BorderClassName, false);
        container.EnableInClassList(BorderClassName, true);
        _previousButtonContainer = container;
    }

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfPlayers });
    }

    public void OnDispose()
    {
        foreach (var kvp in ButtonRegistry)
        {
            var button = kvp.Value.Button;
            var onClick = kvp.Value.OnClick;
            var onBorderChange = kvp.Value.OnBorderChange;
            kvp.Value.Button.clickable.clicked -= onClick;
            kvp.Value.Button.clickable.clicked -= onBorderChange;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
