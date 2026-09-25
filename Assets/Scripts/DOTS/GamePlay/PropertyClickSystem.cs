using Assets.Scripts.DOTS.Characters;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(RayCastSystem))]
    public partial struct PropertyClickSystem : ISystem
    {
        private uint processedSequence;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalPropertyTapResult>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tap = SystemAPI.GetSingleton<LocalPropertyTapResult>();
            if (tap.Sequence == 0 || tap.Sequence == processedSequence) return;
            processedSequence = tap.Sequence;
            if (tap.BlockedByUI) return;

            foreach (var selected in SystemAPI.Query<RefRW<ClickedPropertyComponent>>().WithAll<GhostOwnerIsLocal>())
                selected.ValueRW.entity = tap.Property;

            if (tap.Property == Entity.Null) return;
            if (SystemAPI.HasComponent<NameComponent>(tap.Property))
            {
                var propertyName = SystemAPI.GetComponent<NameComponent>(tap.Property).Value;
                UnityEngine.Debug.Log($"[PropertyTap] Tapped property: {propertyName} ({tap.Property}).");
            }
            else
                UnityEngine.Debug.Log($"[PropertyTap] Tapped property: {tap.Property}.");
        }
    }
}
