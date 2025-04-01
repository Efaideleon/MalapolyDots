using Unity.Burst;
using Unity.Entities;

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
        state.RequireForUpdate<CharacterSelectedBuffer>();
        state.RequireForUpdate<PlayerID>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<CurrentPlayerID>(),
        });

        int currentPlayerID = default;

        var firstCharacter = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>()[0].Value;
        foreach (var (playerName, playerID) in SystemAPI.Query<RefRO<NameComponent>, RefRW<PlayerID>>())
        {
            if (firstCharacter == playerName.ValueRO.Value)
            {
                currentPlayerID = playerID.ValueRO.Value;
            }
        }

        SystemAPI.SetComponent(entity, new CurrentPlayerID
        {
            Value = currentPlayerID
        });
    }
}
