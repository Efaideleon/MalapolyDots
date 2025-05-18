using System;
using Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities;

public class HidePanelState : State
{
    private readonly IPanel _panel; 
    public bool IsPlaying = false; 
    public Action OnFinishedPlaying;
    public bool _isCurrentState = false;

    public HidePanelState(IPanel panel)
    { 
        _panel = panel;
    }

    public void UpdateIsPlaying(bool value)
    {
        if (_isCurrentState)
        {
            IsPlaying = value;
        }
    }

    public override void Enter()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("Entering HideState");
#endif
        _isCurrentState = true;
        IsPlaying = true;
    }

    public override void Execute()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("Executing HideState");
#endif
        _panel.Hide();
    }

    public override void Exit()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("Exitting HideState");
#endif
        _isCurrentState = false;
    }
}
