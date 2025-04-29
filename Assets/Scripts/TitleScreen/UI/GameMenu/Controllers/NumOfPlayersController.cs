using System;
using System.Collections.Generic;
using UI.GameMenu;
using Unity.Entities;

public class ButtonPlayer
{
    public ButtonPlayerElement ButtonElement;
    public Action OnClick;
    public Action OnSelect;

    public ButtonPlayer(ButtonPlayerElement buttonElement, Action onClick, Action onSelect)
    {
        ButtonElement = buttonElement;
        OnClick = onClick;
        OnSelect = onSelect;
    }
}

public class NumOfPlayersController
{
    public NumberOfPlayersScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private readonly SelectionHighlighter<ButtonPlayerElement> _selectionHighlighter;
    private readonly List<ButtonPlayer> ButtonRegistry = new();

    public NumOfPlayersController(NumberOfPlayersScreen screen)
    {
        Screen = screen;
        _selectionHighlighter = new(e => e.EnableBorder(), e => e.DisableBorder());
        foreach (var buttonPlayerElement in Screen.ButtonPlayerElements)
        {
            void OnClick() => DispatchDataEvent(buttonPlayerElement.Value);
            void OnSelect() => _selectionHighlighter.Select(buttonPlayerElement);
            ButtonRegistry.Add(new ButtonPlayer(buttonPlayerElement, OnClick, OnSelect));
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
            buttonPlayer.ButtonElement.Button.clickable.clicked += buttonPlayer.OnSelect;
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
            buttonPlayer.ButtonElement.Button.clickable.clicked -= buttonPlayer.OnSelect;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
