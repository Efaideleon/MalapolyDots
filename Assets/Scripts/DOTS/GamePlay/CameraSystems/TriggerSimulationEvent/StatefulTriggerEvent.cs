using Unity.Entities;
using Unity.Physics;

namespace DOTS.GamePlay.CameraSystems.TriggerSimulationEvent
{
    public struct StatefulTriggerEvent : IBufferElementData, ISimulationEvent<StatefulTriggerEvent>, IStatefulEvent
    {
        public Entity EntityA { get; set; }
        public Entity EntityB { get; set; }
        public int BodyIndexA { get; set; }
        public int BodyIndexB { get; set; }
        public ColliderKey ColliderKeyA { get; set; }
        public ColliderKey ColliderKeyB { get; set; }
        public StateEventType State { get; set; }

        public StatefulTriggerEvent(TriggerEvent evt)
        {
            EntityA = evt.EntityA;
            EntityB = evt.EntityB;
            BodyIndexA = evt.BodyIndexA;
            BodyIndexB = evt.BodyIndexB;
            ColliderKeyA = evt.ColliderKeyA;
            ColliderKeyB = evt.ColliderKeyB;
            State = default;
        }

        public int CompareTo(StatefulTriggerEvent other)
        {
            return ISimulationEventUtilities.CompareEvents(this, other);
        }
    }
}
