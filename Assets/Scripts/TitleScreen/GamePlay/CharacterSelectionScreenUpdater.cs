using Unity.Entities;

public struct CharacterButtonEventBufffer : IBufferElementData
{
    public CharacterButton CharacterButton;
}

public partial struct CharacterSelectionScreenUpdater : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentPlayerNumberPickingCharacter>();
        state.RequireForUpdate<TitleScreenControllers>();
        state.RequireForUpdate<CharacterButtonEventBufffer>();
        state.EntityManager.CreateSingletonBuffer<CharacterButtonEventBufffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in
                SystemAPI.Query<
                DynamicBuffer<CharacterButtonEventBufffer
                >>()
                .WithChangeFilter<CharacterButtonEventBufffer>())
        {
            var controllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (controllers == null)
                break;
            if (controllers.CharacterSelectionControler == null)
                break;
            var tempContext = controllers.CharacterSelectionControler.Context;
            foreach (var e in buffer)
            {
                tempContext.CharacterButton = e.CharacterButton;
                controllers.CharacterSelectionControler.Context = tempContext;
                controllers.CharacterSelectionControler.Update();
            }
            buffer.Clear();
        }

        foreach (var playerNumber in
                SystemAPI.Query<
                    RefRO<CurrentPlayerNumberPickingCharacter>
                >()
                .WithChangeFilter<CurrentPlayerNumberPickingCharacter>())
        {
            var controllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (controllers == null)
                break;
            if (controllers.CharacterSelectionControler == null)
                break;

            controllers.CharacterSelectionControler.Context = new CharacterSelectionContext
            {
                PlayerNumber = playerNumber.ValueRO.Value
            };
            controllers.CharacterSelectionControler.Update();
        }
    }
}
