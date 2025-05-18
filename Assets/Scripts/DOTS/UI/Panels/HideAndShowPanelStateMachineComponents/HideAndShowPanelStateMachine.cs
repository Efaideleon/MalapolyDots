using System;
using Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities;
using NUnit.Framework.Constraints;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.Panels.HideAndShowPanelStateMachineComponents
{
    public enum PanelVisibility 
    {
        Hidden,
        Showing
    }

    public class HideAndShowPanelStateMachine
    {
        private readonly StateMachine stateMachine;
        private readonly VisualElement _lastElementToAnimate;
        private readonly IPanel _panel;
        private readonly HidePanelState hideState;
        private readonly ShowPanelState showState; 
        private PanelVisibility _currentVisibility;
        private PanelVisibility? _pendingVisibility = null;
        private static readonly StylePropertyName styleTranslate = new("translate");

        public HideAndShowPanelStateMachine(IPanel panel, VisualElement lastElementToAnimate)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _lastElementToAnimate = lastElementToAnimate ?? throw new ArgumentNullException(nameof(lastElementToAnimate));

            hideState = new(_panel);
            showState = new(_panel);

            var toHide = new Transition(hideState, IsHideNext);
            var toShow = new Transition(showState, IsShowingNext);

            hideState.AddTransition(toShow);
            showState.AddTransition(toHide);

            State[] states = new State[] { hideState, showState };
            stateMachine = new StateMachine(states);
            stateMachine.InitializeToState(hideState);
            _currentVisibility = PanelVisibility.Hidden;
            SetupCallbacks();
        }

        private void SetupCallbacks() => _lastElementToAnimate.RegisterCallback<TransitionEndEvent>(TransitionEndHandler); 

        private void OnTransitionEnd()
        {
            showState.UpdateIsPlaying(false);
            hideState.UpdateIsPlaying(false);
            if (_pendingVisibility.HasValue && _pendingVisibility.Value != _currentVisibility)
            {
                _currentVisibility = _pendingVisibility.Value;
                _pendingVisibility = null;
                stateMachine.Execute();
            }
        }

        private bool IsHideNext() => _currentVisibility == PanelVisibility.Hidden && !CurrentStateIsAnimating(); 
        private bool IsShowingNext() => _currentVisibility == PanelVisibility.Showing && !CurrentStateIsAnimating(); 

        public void Hide() => SetVisibility(PanelVisibility.Hidden); 
        public void Show() => SetVisibility(PanelVisibility.Showing); 

        private void SetVisibility(PanelVisibility target)
        {
            if (CurrentStateIsAnimating() && _currentVisibility != target)
            {
                _pendingVisibility = target;
            }
            else if (_currentVisibility != target) 
            {
                _currentVisibility = target;
                stateMachine.Execute();
            }
        }

        private bool CurrentStateIsAnimating() => hideState.IsPlaying || showState.IsPlaying; 

        private void TransitionEndHandler(TransitionEndEvent e)
        {
            if (e.stylePropertyNames.Contains(styleTranslate)) 
                OnTransitionEnd(); 
            else
            {
                UnityEngine.Debug.LogError($"Transition does include {styleTranslate}");
            }
        }

        public void Dispose() => _lastElementToAnimate.UnregisterCallback<TransitionEndEvent>(TransitionEndHandler); 
    }
}
