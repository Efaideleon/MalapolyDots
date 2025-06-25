using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public enum ActionType
{
    Showing,
    Hiding,
    None
}

public struct HideAndShowAction
{
    public Action Action { get; set; }
    public ActionType Type {get; set; }
}

public class HideAndShowPanelController
{
    public VisualElement Panel { get; private set; }
    private readonly VisualElement _lastElementToAnimate;
    private readonly Queue<HideAndShowAction> _actions = new();
    public ActionType lastActionPlayed = ActionType.None;
    public bool IsPlaying = false;
    private readonly StylePropertyName styleTranslate = new("translate");
    private ActionType _lastActionTypeAdded = ActionType.None;

    public HideAndShowPanelController(VisualElement panel, VisualElement lastElementToAnimate)
    {
        Panel = panel;
        _lastElementToAnimate = lastElementToAnimate;
        SetupCallbacks();
    }

    public void ExecuteAction(HideAndShowAction action)
    {
        if (!IsPlaying && lastActionPlayed != action.Type)
        {
            UnityEngine.Debug.Log($"[HideAndShowPanelController] | Playing action: {action.Type}");
            IsPlaying = true;
            action.Action();
            lastActionPlayed = action.Type;
        }
        else if (IsPlaying && lastActionPlayed == action.Type && _actions.Count < 1)
        {
            UnityEngine.Debug.Log($"[HideAndShowPanelController] | lastActionPlayed: {lastActionPlayed}");
        }
        else if (!IsPlaying && lastActionPlayed == action.Type)
        {
            UnityEngine.Debug.Log($"[HideAndShowPanelController] | lastActionPlayed2: {lastActionPlayed}");
        }
        else
        {
            if (CanQueue(action.Type))
            {
                UnityEngine.Debug.Log($"[HideAndShowPanelController] | Queuing Action: {action.Type}");
                _actions.Enqueue(action);
                _lastActionTypeAdded = action.Type;
            }
        }
    }

    private void SetupCallbacks()
    {
        _lastElementToAnimate.RegisterCallback<TransitionEndEvent>(e => 
        {
            if (e.stylePropertyNames.Contains(styleTranslate)) 
            {
                UnityEngine.Debug.Log("[HideAndShowPanelController] | Animation Ended");
                if (_actions.Count > 0)
                {
                    var action = _actions.Dequeue();
                    UnityEngine.Debug.Log($"[HideAndShowPanelController] | Playing next animation: {action.Type}");
                    action.Action();
                    lastActionPlayed = action.Type;
                }
                else
                {
                    IsPlaying = false;
                    UnityEngine.Debug.Log("[HideAndShowPanelController] | actions queue has been emptied");
                    _lastActionTypeAdded = ActionType.None;
                }
            }
        });
    }

    private bool CanQueue(ActionType type)
    {
        if (type == _lastActionTypeAdded)
            return false;
        return true;
    }
}
