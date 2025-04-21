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
            // Initializing Event communication to buy houses
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<BuyHouseEventBuffer>(entity);

            // Initializing Roll Event Bus
            var rollBusBufferEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<RollEventBuffer>(rollBusBufferEntity);

            // Initializing Roll Event Bus
            var transactionEventEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<TransactionEventBuffer>(transactionEventEntity);

            // Initializing the Current Player ID
            state.RequireForUpdate<CharacterSelectedBuffer>();
            state.RequireForUpdate<PlayerID>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var firstCharacter = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>()[0].Value;
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
