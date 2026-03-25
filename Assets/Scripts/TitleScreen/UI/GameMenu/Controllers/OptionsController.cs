using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.UIElements;

public class SelectableButtonElement
{
    private const string BorderClassName = "dbr-btn-picked";
    public readonly VisualElement Border; 
    public readonly Button Button; 
    public readonly int Value;
    public SelectableButtonElement(Button button, int value)
    {
        Button = button;
        Border = Button.parent;
        Value = value;
    }

    public void EnableBorder() => Border?.EnableInClassList(BorderClassName, true); 
    public void DisableBorder() => Border?.EnableInClassList(BorderClassName, false);
}

public class SelectableButton
{
    public SelectableButtonElement ButtonElement;
    public Action OnClick;
    public Action OnSelect;

    public SelectableButton(SelectableButtonElement buttonElement, Action onClick, Action onSelect)
    {
        ButtonElement = buttonElement;
        OnClick = onClick;
        OnSelect = onSelect;
    }
}

public interface IOptionsScreen
{
    public SelectableButtonElement[] GetSelectableButtonElements();
    public Button ConfirmButton { get; set; }
    public void Show();
    public void Hide();

}

public class OptionsController
{
    public IOptionsScreen Screen { get; private set; }
    private EntityQuery _changeScreenQuery;
    private readonly List<SelectableButton> ButtonRegistry = new();
    private readonly SelectionHighlighter<SelectableButtonElement> _selectionHighlighter;
    private readonly Action<int> DispatchDataEvent;
    private readonly ScreenType _screenType;

    public OptionsController(IOptionsScreen screen, Action<int> dispatchDataEvent, EntityQuery changeScreenQuery, ScreenType screenType)
    {
        Screen = screen;
        _screenType = screenType;
        DispatchDataEvent = dispatchDataEvent;
        _changeScreenQuery = changeScreenQuery;
        _selectionHighlighter = new(e => e.EnableBorder(), e => e.DisableBorder());
        foreach (var selectableButtonElement in Screen.GetSelectableButtonElements())
        {
            void OnClick() => DispatchDataEvent(selectableButtonElement.Value);
            void OnSelect() => _selectionHighlighter.Select(selectableButtonElement);
            ButtonRegistry.Add(new SelectableButton(selectableButtonElement, OnClick, OnSelect));
        }
        SubscribeEvents();
    }

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
        {
            _changeScreenQuery.GetSingletonBuffer<ChangeScreenEventBuffer>()
                .Add(new ChangeScreenEventBuffer { ScreenType = _screenType });
            UnityEngine.Debug.Log($"[OptionsController] | ScreenType: {_screenType}");
        }
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
