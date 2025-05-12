using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HideAndShowPanelController
{
    public VisualElement Panel { get; private set; }
    private readonly VisualElement _lastElementToAnimate;
    private readonly Queue<Action> _actions = new();
    private bool _isPlaying = false;
    private readonly StylePropertyName styleTranslate = new("translate");

    public HideAndShowPanelController(VisualElement panel, VisualElement lastElementToAnimate)
    {
        Panel = panel;
        _lastElementToAnimate = lastElementToAnimate;
        SetupCallbacks();
    }

    public void ExecuteAction(Action action)
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            action();
        }
        else
        {
            _actions.Enqueue(action);
        }
    }

    private void SetupCallbacks()
    {
        _lastElementToAnimate.RegisterCallback<TransitionEndEvent>(e => 
        {
            if (e.stylePropertyNames.Contains(styleTranslate)) 
            {
                if (_actions.Count > 0)
                {
                    var action = _actions.Dequeue();
                    action();
                }
                else
                    _isPlaying = false;
            }
        });
    }
}
