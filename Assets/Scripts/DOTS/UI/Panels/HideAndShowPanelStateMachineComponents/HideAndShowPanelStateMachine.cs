using System;
using Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities;
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
        private PanelVisibility _targetVisibility;
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
            _targetVisibility = PanelVisibility.Hidden;
            SetupCallbacks();
        }

        private void SetupCallbacks() => _lastElementToAnimate.RegisterCallback<TransitionEndEvent>(TransitionEndHandler);

        private void OnTransitionEnd()
        {
            showState.UpdateIsPlaying(false);
            hideState.UpdateIsPlaying(false);
            if (_pendingVisibility.HasValue && _pendingVisibility.Value != _targetVisibility)
            {
                _targetVisibility = _pendingVisibility.Value;
                _pendingVisibility = null;
                SafeExecute(stateMachine.Execute);
            }
        }

        private bool IsHideNext() => _targetVisibility == PanelVisibility.Hidden && !CurrentStateIsAnimating();
        private bool IsShowingNext() => _targetVisibility == PanelVisibility.Showing && !CurrentStateIsAnimating();

        public void Hide() => SetVisibility(PanelVisibility.Hidden);
        public void Show() => SetVisibility(PanelVisibility.Showing);

        private void SetVisibility(PanelVisibility target)
        {
            if (CurrentStateIsAnimating() && _targetVisibility != target)
            {
                _pendingVisibility = target;
            }
            else if (_targetVisibility != target)
            {
                _targetVisibility = target;
                SafeExecute(stateMachine.Execute);
            }
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"No visibility is set, panel is already in {target}");
#endif
            }
        }

        private bool CurrentStateIsAnimating()
        {
#if UNITY_EDITOR
            if (hideState.IsPlaying && showState.IsPlaying)
                throw new InvalidOperationException($"hideState.IsPlaying and showState.IsPlaying can't both be true");
#endif
            return hideState.IsPlaying || showState.IsPlaying;
        }

        private void TransitionEndHandler(TransitionEndEvent e)
        {
            if (e.stylePropertyNames.Contains(styleTranslate))
                OnTransitionEnd();
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogError($"Transition does not include {styleTranslate}");
#endif
            }
        }

        private void SafeExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"StateMachine execution failed during {action.Method.Name}: {ex}");
            }
        }

        public void Dispose() => _lastElementToAnimate.UnregisterCallback<TransitionEndEvent>(TransitionEndHandler);
    }
}
