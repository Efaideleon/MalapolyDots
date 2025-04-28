using System;
using System.Collections.Generic;
using UI.GameMenu;
using Unity.Entities;

public class ButtonPlayer
{
    public ButtonPlayerElement ButtonElement;
    public Action OnClick;
    public Action OnBorderChange;

    public ButtonPlayer(ButtonPlayerElement buttonElement, Action onClick, Action onBorderChange)
    {
        ButtonElement = buttonElement;
        OnClick = onClick;
        OnBorderChange = onBorderChange;
    }
}

public class NumOfPlayersController
{
    public NumberOfPlayersScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private ButtonPlayerElement _previousButton = null;
    private readonly List<ButtonPlayer> ButtonRegistry = new();

    public NumOfPlayersController(NumberOfPlayersScreen screen)
    {
        Screen = screen;
        foreach (var buttonPlayerElement in Screen.ButtonPlayerElements)
        {
            void OnClick() => DispatchDataEvent(buttonPlayerElement.Value);
            void OnBorderChange() => UpdateBorder(buttonPlayerElement);
            ButtonRegistry.Add(new ButtonPlayer(buttonPlayerElement, OnClick, OnBorderChange));
        }
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventQuery = query;
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query;
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void SubscribeEvents()
    {
        foreach (var buttonPlayer in ButtonRegistry)
        {
            buttonPlayer.ButtonElement.Button.clickable.clicked += buttonPlayer.OnClick;
            buttonPlayer.ButtonElement.Button.clickable.clicked += buttonPlayer.OnBorderChange;
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

    private void UpdateBorder(ButtonPlayerElement button)
    {
        _previousButton?.DisableBorder();
        button.EnableBorder();
        _previousButton = button;
    }

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfPlayers });
    }

    public void OnDispose()
    {
        foreach (var buttonPlayer in ButtonRegistry)
        {
            buttonPlayer.ButtonElement.Button.clickable.clicked -= buttonPlayer.OnClick;
            buttonPlayer.ButtonElement.Button.clickable.clicked -= buttonPlayer.OnBorderChange;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
