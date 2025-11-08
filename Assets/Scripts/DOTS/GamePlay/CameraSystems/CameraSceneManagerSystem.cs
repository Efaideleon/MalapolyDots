using DOTS.Characters;
using DOTS.GamePlay.CameraSystems.TriggerSimulationEvent;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct CameraSceneManagerSystem : ISystem
    {
        private ComponentLookup<CameraSceneTag> cameraSceneTags;
        private ComponentLookup<CharacterFlag> characterFlags;

        private NativeList<StatefulTriggerEvent> CurrentEvents;
        private NativeList<StatefulTriggerEvent> PreviousEvents;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraSceneTag>();
            state.RequireForUpdate<CharacterFlag>();

            state.EntityManager.CreateSingletonBuffer<StatefulTriggerEvent>();

            CurrentEvents = new NativeList<StatefulTriggerEvent>(Allocator.Persistent);
            PreviousEvents = new NativeList<StatefulTriggerEvent>(Allocator.Persistent);

            cameraSceneTags = state.GetComponentLookup<CameraSceneTag>(true);
            characterFlags = state.GetComponentLookup<CharacterFlag>(true);
        }

        [BurstCompile]
        private partial struct CountNumTriggerEvents : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<CameraSceneTag> cameraSceneTags;
            [ReadOnly] public ComponentLookup<CharacterFlag> characterFlags;

            public NativeList<StatefulTriggerEvent> statefulEventList;

            public void Execute(TriggerEvent triggerEvent)
            {
                if (IsPlayerOnCameraZone(in triggerEvent))
                {
                    statefulEventList.Add(new StatefulTriggerEvent(triggerEvent));
                }
            }

            private bool IsPlayerOnCameraZone(in TriggerEvent evt) =>
                characterFlags.HasComponent(evt.EntityA) && cameraSceneTags.HasComponent(evt.EntityB) ||
                characterFlags.HasComponent(evt.EntityB) && cameraSceneTags.HasComponent(evt.EntityA);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            cameraSceneTags.Update(ref state);
            characterFlags.Update(ref state);

            NativeList<StatefulTriggerEvent> statefulEvents = new(Allocator.Temp);
            var buffer = SystemAPI.GetSingletonBuffer<StatefulTriggerEvent>();

            (CurrentEvents, PreviousEvents) = (PreviousEvents, CurrentEvents);
            CurrentEvents.Clear();

            var job = new CountNumTriggerEvents()
            {
                cameraSceneTags = cameraSceneTags,
                characterFlags = characterFlags,
                statefulEventList = CurrentEvents
            };
            var jobHandle = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            jobHandle.Complete();

            GetStatefulEvent(in PreviousEvents, in CurrentEvents, statefulEvents);
            CopyNativeListToDynamicBuffer(statefulEvents, buffer, clear: true);
        }

        [BurstCompile]
        private readonly void CopyNativeListToDynamicBuffer<T>(
                NativeList<T> list,
                DynamicBuffer<T> buffer,
                bool clear
        ) where T : unmanaged, IBufferElementData
        {
            if (clear) buffer.Clear();

            foreach (var e in list)
            {
                buffer.Add(e);
            }
        }

        [BurstCompile]
        private readonly void GetStatefulEvent(
                in NativeList<StatefulTriggerEvent> previousEvents,
                in NativeList<StatefulTriggerEvent> currentEvents,
                NativeList<StatefulTriggerEvent> statefulEvents)
        {
            var p = 0;
            var c = 0;

            while (p < previousEvents.Length && c < currentEvents.Length)
            {
                var r = previousEvents[p].CompareTo(currentEvents[c]);

                if (r == 0)
                {
                    var tempEvent = currentEvents[c];
                    tempEvent.State = StateEventType.Stay;
                    statefulEvents.Add(tempEvent);
                    c++;
                    p++;
                }
                else if (r < 0)
                {
                    var tempPreviousEvent = previousEvents[p];
                    tempPreviousEvent.State = StateEventType.Exit;
                    statefulEvents.Add(tempPreviousEvent);
                    p++;
                }
                else
                {
                    var tempEvent = currentEvents[c];
                    tempEvent.State = StateEventType.Enter;
                    statefulEvents.Add(tempEvent);
                    c++;
                }
            }

            if (c == currentEvents.Length)
            {
                while (p < previousEvents.Length)
                {
                    var tempPreviousEvent = previousEvents[p];
                    tempPreviousEvent.State = StateEventType.Exit;
                    statefulEvents.Add(tempPreviousEvent);
                    p++;
                }
            }

            if (p == previousEvents.Length)
            {
                while (c < currentEvents.Length)
                {
                    var tempEvent = currentEvents[c];
                    tempEvent.State = StateEventType.Enter;
                    statefulEvents.Add(tempEvent);
                    c++;
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            if (CurrentEvents.IsCreated) CurrentEvents.Dispose();
            if (PreviousEvents.IsCreated) PreviousEvents.Dispose();
        }
    }
}
