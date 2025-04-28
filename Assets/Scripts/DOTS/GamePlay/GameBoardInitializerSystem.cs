using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GameData;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct CurrentPlayerID : IComponentData
    {
        public int Value;
    }

    [BurstCompile]
    public partial struct GameBoardInitializerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Initializing the Current Player ID
            state.RequireForUpdate<CharacterSelectedNameBuffer>();
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<IsChangingToGameScene>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.GetSingleton<IsChangingToGameScene>().Value)
            {
                state.Enabled = false;

                var firstCharacter = SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>()[0].Name;
                foreach (var (playerName, playerID, e) in 
                        SystemAPI.Query<
                        RefRO<NameComponent>,
                        RefRW<PlayerID>
                        >()
                        .WithEntityAccess())
                {
                    if (firstCharacter == playerName.ValueRO.Value)
                    {
                        var currentPlayerID = playerID.ValueRO.Value;
                        state.EntityManager.CreateSingleton( new CurrentPlayerComponent { entity = e });
                        state.EntityManager.CreateSingleton( new CurrentPlayerID { Value = currentPlayerID });
                    }
                }
            }
        }
    }
}
