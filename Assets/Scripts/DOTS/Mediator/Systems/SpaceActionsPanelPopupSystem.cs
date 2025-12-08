using DOTS.Characters;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

public struct ShowActionsPanelBuffer : IBufferElementData
{ }

[BurstCompile]
public partial struct SpaceActionsPanelPopupSystem : ISystem
{
    public ComponentLookup<GameStateComponent> gameStateLookup;
    public ComponentLookup<PropertySpaceTag> propertySpaceLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<CurrentActivePlayer>();
        state.RequireForUpdate<SpaceLandedOn>();

        state.EntityManager.CreateSingletonBuffer<ShowActionsPanelBuffer>();

        gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>();
        propertySpaceLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        gameStateLookup.Update(ref state);
        propertySpaceLookup.Update(ref state);

        var gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
        if (gameStateLookup.HasComponent(gameStateEntity) && gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion))
        {
            if (gameStateLookup[gameStateEntity].State == GameState.Landing)
            {
                var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

                if (SystemAPI.HasComponent<SpaceLandedOn>(activePlayerEntity))
                {
                    var landedOnEntity = SystemAPI.GetComponent<SpaceLandedOn>(activePlayerEntity).entity;
                    if (propertySpaceLookup.HasComponent(landedOnEntity))
                        SystemAPI.GetSingletonBuffer<ShowActionsPanelBuffer>().Add(new ShowActionsPanelBuffer { });
                }
            }
        }
    }
}
