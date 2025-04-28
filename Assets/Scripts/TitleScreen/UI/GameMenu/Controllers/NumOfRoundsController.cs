using System;
using System.Collections.Generic;
using Unity.Entities;

public class RoundsButton
{
    public NumOfRoundsButtonElement ButtonElement;
    public Action OnClick;
    public Action OnBorderChange;

    public RoundsButton(NumOfRoundsButtonElement buttonElement, Action onClick, Action onBorderChange)
    {
        ButtonElement = buttonElement;
        OnClick = onClick;
        OnBorderChange = onBorderChange;
    }
}

public class NumOfRoundsController
{
    public NumOfRoundsScreen Screen { get; private set; }
    private EntityQuery _dataEventQuery;
    private EntityQuery _changeScreenQuery;
    private readonly List<RoundsButton> ButtonRegistry = new();
    private NumOfRoundsButtonElement _previousButton = null;

    public NumOfRoundsController(NumOfRoundsScreen screen)
    {
        Screen = screen;
        foreach (var RoundsButtonElement in Screen.RoundsButtonElements)
        {
            void OnClick() => DispatchDataEvent(RoundsButtonElement.Value);
            void OnBorderChange() => UpdateBorder(RoundsButtonElement);
            ButtonRegistry.Add(new RoundsButton(RoundsButtonElement, OnClick, OnBorderChange));
        }
        SubscribeEvents();
    }

    public void SetDataEventBufferQuery(EntityQuery query) => _dataEventQuery = query; 
    public void SetChangeScreenEventBufferQuery(EntityQuery query) => _changeScreenQuery = query; 
    public void ShowScreen() => Screen.Show();
    public void HideScreen() => Screen.Hide();

    private void UpdateBorder(NumOfRoundsButtonElement button)
    {
        _previousButton?.DisableBorder();
        button.EnableBorder();
        _previousButton = button;
    }

    private void SubscribeEvents()
    {
        foreach (var buttonPlayer in ButtonRegistry)
        {
            buttonPlayer.ButtonElement.Button.clickable.clicked += buttonPlayer.OnClick;
            buttonPlayer.ButtonElement.Button.clickable.clicked += buttonPlayer.OnBorderChange;
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
            buttonPlayer.ButtonElement.Button.clickable.clicked -= buttonPlayer.OnBorderChange;
        }
        Screen.ConfirmButton.clickable.clicked -= DispatchScreenChangeEvent;
    }
}
