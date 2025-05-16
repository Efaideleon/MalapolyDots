namespace Assets.Scripts.DOTS.UI.Panels.StateMachineUtilities
{
    public class Transition
    {
        public State FromState;
        public State ToState;
    }

    public class State
    {
        public  State()
        {}

        public void Execute()
        {}
    }

    public class StateMachine 
    {
        public State[] States;
        public State CurrentState { get; private set; }
        public Transition[] Transitions;

        public StateMachine(int numOfStates)
        {
            States = new State[numOfStates];
        }
    }
}
