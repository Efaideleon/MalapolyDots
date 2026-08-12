using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct JailSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    foreach (var (spaceLandedOn, jailState) in SystemAPI.Query<RefRO<SpaceLandedOn>, RefRW<JailState>>().WithAll<ActivePlayer>())
                    {
                        var landedOnEntity = spaceLandedOn.ValueRO.entity;

                        // If landed on a treasure spot.
                        if (SystemAPI.HasComponent<JailSpaceTag>(landedOnEntity))
                        {
                            jailState.ValueRW.InJail = true;
                            jailState.ValueRW.TurnsInJail = 2;
                        }
                    }
                }
            }
        }
    }
}
