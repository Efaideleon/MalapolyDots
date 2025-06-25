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
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[ShowPanelState] | Entering ShowState");
#endif
        IsPlaying = true;
        _isCurrentState = true;
    }

    public override void Execute()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[ShowPanelState] | Executing ShowState");
#endif
        _panel.Show();
    }

    public override void Exit()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.Log("[ShowPanelState] | Exiting ShowState");
#endif
        _isCurrentState = false;
    }
}
