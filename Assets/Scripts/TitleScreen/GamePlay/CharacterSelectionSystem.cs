using Unity.Entities;

public enum AvailableState
{
    Available,
    Unavailable
}

public struct Available : IComponentData
{
    public AvailableState Value;
}

public struct CharacterType : IComponentData
{
    public Character Value;
}

public struct ConfirmButtonEventBuffer : IBufferElementData
{
    public Character character;
}

public partial struct CharacterSelectionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        foreach (var characterType in CharacterData.AllCharacters)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<Available>(),
                ComponentType.ReadOnly<CharacterType>(),
            });
            SystemAPI.SetComponent(entity, new Available { Value = AvailableState.Available });
            SystemAPI.SetComponent(entity, new CharacterType { Value = characterType });
        }
        state.EntityManager.CreateSingletonBuffer<ConfirmButtonEventBuffer>();
        state.RequireForUpdate<Available>();
        state.RequireForUpdate<CharacterType>();
        state.RequireForUpdate<TitleScreenControllers>();
        state.RequireForUpdate<ConfirmButtonEventBuffer>();
        state.RequireForUpdate<CharacterSelectedNameBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    { 
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<ConfirmButtonEventBuffer>>().WithChangeFilter<ConfirmButtonEventBuffer>())
        {
            var controllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (controllers == null)
                break;
            if (controllers.CharacterSelectionControler == null)
                break;

            foreach (var e in buffer)
            {
                foreach (var (characterType, available) in SystemAPI.Query<RefRO<CharacterType>, RefRW<Available>>())
                {
                    var availableState = available.ValueRO.Value;
                    if (e.character == characterType.ValueRO.Value)
                    {
                        var tempContext = controllers.CharacterSelectionControler.Context;
                        if (availableState == AvailableState.Available)
                        {
                            SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>()
                                .Add(new CharacterSelectedNameBuffer { Name = characterType.ValueRO.Value.ToString()});

                            available.ValueRW.Value = AvailableState.Unavailable;
                            tempContext.CharacterButton.Type = characterType.ValueRO.Value;
                            tempContext.CharacterButton.State = available.ValueRO.Value;
                            controllers.CharacterSelectionControler.Context = tempContext;
                            controllers.CharacterSelectionControler.Update();
                        }
                    }
                }
            }
            buffer.Clear();
        }
    }
}
