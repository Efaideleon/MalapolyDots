using Unity.Entities;

public struct ConfirmButtonEventBuffer : IBufferElementData
{
    public Character character;
}

public partial struct CharacterSelectionScreenUpdater : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentPlayerNumberPickingCharacter>();
        state.RequireForUpdate<TitleScreenControllers>();
        state.RequireForUpdate<ConfirmButtonEventBuffer>();
        state.EntityManager.CreateSingletonBuffer<ConfirmButtonEventBuffer>();
    }

    public void OnUpdate(ref SystemState state)
    {
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
