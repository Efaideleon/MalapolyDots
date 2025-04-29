using System;
using System.Collections.Generic;
using Unity.Entities;

public class RoundsButton
{
    public NumOfRoundsButtonElement ButtonElement;
    public Action OnClick;
    public Action OnSelect;

    public RoundsButton(NumOfRoundsButtonElement buttonElement, Action onClick, Action onSelect)
    {
        ButtonElement = buttonElement;
        OnClick = onClick;
        OnSelect = onSelect;
    }
}

public class NumOfRoundsController
{
    public NumOfRoundsScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private readonly List<RoundsButton> ButtonRegistry = new();
    private readonly SelectionHighlighter<NumOfRoundsButtonElement> _selectionHighlighter;

    public NumOfRoundsController(NumOfRoundsScreen screen)
    {
        Screen = screen;
        _selectionHighlighter = new(e => e.EnableBorder(), e => e.DisableBorder());
        foreach (var RoundsButtonElement in Screen.RoundsButtonElements)
        {
            void OnClick() => DispatchDataEvent(RoundsButtonElement.Value);
            void OnSelect() => _selectionHighlighter.Select(RoundsButtonElement);
            ButtonRegistry.Add(new RoundsButton(RoundsButtonElement, OnClick, OnSelect));
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

    private void DispatchScreenChangeEvent()
    {
        if (_changeScreenQuery != null)
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfRounds });
    }

    private void DispatchDataEvent(int num)
    {
        if (_dataEventQuery != null)
            _dataEventQuery.GetSingletonBuffer<NumberOfRoundsEventBuffer>()
                .Add(new NumberOfRoundsEventBuffer { NumberOfRounds = num });
        else
            UnityEngine.Debug.LogWarning("_eventBufferQuery not set in NumOfPlayersController");
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
