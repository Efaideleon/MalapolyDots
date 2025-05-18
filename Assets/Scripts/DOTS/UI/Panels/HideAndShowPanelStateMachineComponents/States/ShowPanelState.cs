using System;
using Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities;

public class ShowPanelState : State
{
    private readonly IPanel _panel; 
    public bool IsPlaying = false; 
    public Action OnFinishedPlaying;
    public bool _isCurrentState = false;

    public ShowPanelState(IPanel panel)
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
        UnityEngine.Debug.Log("Entering ShowState");
        IsPlaying = true;
        _isCurrentState = true;
    }

    public override void Execute()
    {
        UnityEngine.Debug.Log("Executing ShowState");
        _panel.Show();
    }

    public override void Exit()
    {
        UnityEngine.Debug.Log("Exitting ShowState");
        _isCurrentState = false;
    }
}
