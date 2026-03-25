using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

public struct CharacterSelectedEventBuffer : IBufferElementData
{
    public CharacterButton CharacterButtonSelected;
}

public struct NumOfPlayerPicking : IComponentData
{
    public int Value;
}

public struct IsCharacterAvailable : IComponentData
{
    public bool Value;
    public CharacterButton CharacterSelectedButton;
}

public struct LastCharacterClicked : IComponentData
{
    public CharacterButton Value;
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
        state.EntityManager.CreateSingleton(new IsCharacterAvailable { Value = false, CharacterSelectedButton = default });
        state.EntityManager.CreateSingleton(new LastCharacterClicked { Value = new CharacterButton { State = AvailableState.Unavailable, Type = Character.None } });
        state.EntityManager.CreateSingleton(new NumOfPlayerPicking { Value = 1 });
        var loginEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<LoginData>(),
            ComponentType.ReadOnly<CharacterSelectedNameBuffer>(),
        });
        SystemAPI.SetComponent(loginEntity, new LoginData { NumberOfRounds = default, NumberOfPlayers = default });
        state.RequireForUpdate<CharacterSelectedEventBuffer>();
        state.RequireForUpdate<NumberOfRoundsEventBuffer>();
        state.RequireForUpdate<NumberOfPlayersEventBuffer>();
        state.RequireForUpdate<IsCharacterAvailable>();
        state.RequireForUpdate<NumOfPlayerPicking>();
        state.RequireForUpdate<CharacterSelectedNameBuffer>();
        state.RequireForUpdate<ConfirmButtonEventBuffer>();
        state.RequireForUpdate<LastCharacterClicked>();
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
                var charactersSelected = e.CharacterButtonSelected;
                SystemAPI.GetSingletonRW<LastCharacterClicked>().ValueRW.Value = charactersSelected;
            }
            eventBuffer.Clear();
        }
    }
}
