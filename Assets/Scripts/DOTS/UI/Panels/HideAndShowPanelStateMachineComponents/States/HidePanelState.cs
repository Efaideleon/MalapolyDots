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
        UnityEngine.Debug.Log("Entering HideState");
        _isCurrentState = true;
        IsPlaying = true;
    }

    public override void Execute()
    {
        UnityEngine.Debug.Log("Executing HideState");
        _panel.Hide();
    }

    public override void Exit()
    {
        UnityEngine.Debug.Log("Exitting HideState");
        _isCurrentState = false;
    }
}
