using System.Linq;
using Unity.Collections;
using Unity.Entities;

public struct CharacterSelectedEventBuffer : IBufferElementData
{
    public FixedString64Bytes Name;
}

public struct CurrentPlayerNumberPickingCharacter : IComponentData
{
    public int Value;
}

public struct IsCharacterAvailable : IComponentData
{
    public bool Value;
    public FixedString64Bytes Name;
}

public struct NumberOfRoundsEventBuffer : IBufferElementData
{
    public int NumberOfRounds;
}

public struct NumberOfPlayersEventBuffer : IBufferElementData
{
    public int NumberOfPlayers;
}

public struct CharacterSelectedNameBuffer : IBufferElementData
{
    public FixedString64Bytes Name;
}

public struct LoginData : IComponentData
{
    public int NumberOfPlayers;
    public int NumberOfRounds;
}

public partial struct LoginSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<CharacterSelectedEventBuffer>();
        state.EntityManager.CreateSingletonBuffer<NumberOfRoundsEventBuffer>();
        state.EntityManager.CreateSingletonBuffer<NumberOfPlayersEventBuffer>();
        state.EntityManager.CreateSingleton( new IsCharacterAvailable { Value = false, Name = default });
        state.EntityManager.CreateSingleton( new CurrentPlayerNumberPickingCharacter { Value = 1 });
        var loginEntity = state.EntityManager.CreateEntity( stackalloc ComponentType[] 
        {
            ComponentType.ReadOnly<LoginData>(),
            ComponentType.ReadOnly<CharacterSelectedNameBuffer>()
        });
        SystemAPI.SetComponent(loginEntity, new LoginData { NumberOfRounds = default, NumberOfPlayers = default});
        state.RequireForUpdate<CharacterSelectedEventBuffer>();
        state.RequireForUpdate<NumberOfRoundsEventBuffer>();
        state.RequireForUpdate<NumberOfPlayersEventBuffer>();
        state.RequireForUpdate<IsCharacterAvailable>();
        state.RequireForUpdate<CurrentPlayerNumberPickingCharacter>();
        state.RequireForUpdate<CharacterSelectedNameBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var eventBuffer in 
                SystemAPI.Query<
                    DynamicBuffer<NumberOfPlayersEventBuffer>
                >()
                .WithChangeFilter<NumberOfPlayersEventBuffer>())
        {
            var titleScreenControllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (titleScreenControllers == null)
                break;
            if (titleScreenControllers.NumOfPlayersController == null)
                break;
            foreach (var e in eventBuffer)
            {
                SystemAPI.GetSingletonRW<LoginData>().ValueRW.NumberOfPlayers = e.NumberOfPlayers;
            }
            eventBuffer.Clear();
        }

        foreach (var eventBuffer in 
                SystemAPI.Query<
                    DynamicBuffer<CharacterSelectedEventBuffer>
                >()
                .WithChangeFilter<CharacterSelectedEventBuffer>())
        {
            var titleScreenControllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (titleScreenControllers == null)
                break;
            if (titleScreenControllers.CharacterSelectionControler == null)
                break;
            foreach (var e in eventBuffer)
            {
                var charactersSelected = SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>();
                if (charactersSelected.Length > 1)
                {
                    foreach (var character in charactersSelected)
                    {
                        if (e.Name == character.Name)
                        {
                            SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Value = false;
                            SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Name = default;
                            UnityEngine.Debug.Log("can't add element to buffer");
                        }
                        else 
                        {
                            UnityEngine.Debug.Log("Adding element to name buffers");
                            SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Value = true;
                            SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Name = e.Name;
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("Adding element to name buffers");
                    SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Value = true;
                    SystemAPI.GetSingletonRW<IsCharacterAvailable>().ValueRW.Name = e.Name;
                }
            }
            eventBuffer.Clear();
        }

        foreach (var eventBuffer in 
                SystemAPI.Query<
                    DynamicBuffer<NumberOfRoundsEventBuffer>
                >()
                .WithChangeFilter<NumberOfRoundsEventBuffer>())
        {
            var titleScreenControllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (titleScreenControllers == null)
                break;
            if (titleScreenControllers.NumOfRoundsController == null)
                break;
            foreach (var e in eventBuffer)
            {
            }
            eventBuffer.Clear();
        }
    }
}
