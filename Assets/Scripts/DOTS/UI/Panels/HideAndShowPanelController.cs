using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class HideAndShowPanelController
{
    public VisualElement Panel { get; private set; }
    private readonly VisualElement _lastElementToAnimate;
    private readonly Queue<Action> _actions = new();
    public bool IsPlaying = false;
    private readonly StylePropertyName styleTranslate = new("translate");

    public HideAndShowPanelController(VisualElement panel, VisualElement lastElementToAnimate)
    {
        Panel = panel;
        _lastElementToAnimate = lastElementToAnimate;
        SetupCallbacks();
    }

    public void ExecuteAction(Action action)
    {
        if (!IsPlaying)
        {
            UnityEngine.Debug.Log("Playing action");
            IsPlaying = true;
            action();
        }
        else
        {
            _actions.Enqueue(action);
            UnityEngine.Debug.Log("Queuing Action");
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
                    IsPlaying = false;
            }
        });
    }
}
