namespace DOTS.GamePlay.CameraSystems.TriggerSimulationEvent
{
    public enum StateEventType
    {
        Undefined,
        Enter,
        Stay,
        Exit
    }

    public interface IStatefulEvent
    {
        public StateEventType State { get; set; }
    }
}
