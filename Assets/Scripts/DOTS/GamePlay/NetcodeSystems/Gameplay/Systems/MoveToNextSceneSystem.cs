using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct MoveToNextSceneSystem : ISystem
    {
        private ComponentLookup<AllLockedIn> allLockedInLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AllLockedIn>();
            allLockedInLookup = SystemAPI.GetComponentLookup<AllLockedIn>();
        }

        public void OnUpdate(ref SystemState state)
        {
            allLockedInLookup.Update(ref state);

            var allLockedInEntity = SystemAPI.GetSingletonEntity<AllLockedIn>();
            if (!allLockedInLookup.DidChange(allLockedInEntity, state.LastSystemVersion))
                return;

            // Move to the next scene.
        }
    }
}

