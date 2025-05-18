using System;
using System.Collections.Generic;

namespace Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities
{
    public class Transition
    {
        public State ToState { get; private set; }
        public Func<bool> Condition { get; private set; }
        public Transition (State toState, Func<bool> condition)
        {
            ToState = toState;
            Condition = condition;
        }
    }

    public abstract class State
    {
        public List<Transition> Transitions = new();

        public abstract void Execute();
        public abstract void Enter();
        public abstract void Exit();

        public void AddTransition(Transition transition)
        {
            Transitions.Add(transition);
        }
    }

    public class StateMachine 
    {
        public State[] States;
        public State CurrentState { get; private set; } = null;

        public StateMachine(State[] states)
        {
            States = states; 
        }

        public void InitializeToState(State state)
        {
            CurrentState = state;
        }

        public void Execute()
        { 
            var newState = GetNewState();
            if (CurrentState != newState) //TODO: how is it checking if it's the same state?
            {
                CurrentState.Exit();
                CurrentState = newState;
                CurrentState.Enter();
                CurrentState.Execute();
            }
        }

        private State GetNewState()
        {
            foreach (var transition in CurrentState.Transitions)
            {
                if (transition.Condition())
                    return transition.ToState;
            }
            return CurrentState;
        }
    }
}
