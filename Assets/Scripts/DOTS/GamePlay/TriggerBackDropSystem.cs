using Assets.Scripts.DOTS.Characters;
using DOTS.EventBuses;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct TriggerBackdropSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<UIButtonDirtyFlag>();
            state.RequireForUpdate<BackDropEventBus>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (bus, clickedProperty, touchStarted) in 
                    SystemAPI.Query<DynamicBuffer<BackDropEventBus>, RefRO<ClickedPropertyComponent>, RefRO<TouchStartedInput>>())
            {
                if (touchStarted.ValueRO.IsTapped.IsSet)
                {
                    // Don't process raycast data if the user clicked an ui element.
                    var wasUIButtonClicked = SystemAPI.GetSingleton<UIButtonDirtyFlag>();
                    if (wasUIButtonClicked.Value)
                        return;

                    UnityEngine.Debug.Log($"[TriggerBackdropSystem] | no ui element clicked.");

                    if (clickedProperty.ValueRO.entity == Entity.Null)
                    {
                        UnityEngine.Debug.Log($"[TriggerBackdropSystem] | sending even backdrop bus.");
                        bus.Add(new BackDropEventBus());
                    }
                }
            }
        }
    }
}
