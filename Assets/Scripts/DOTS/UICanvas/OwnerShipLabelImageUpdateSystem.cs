using DOTS.Characters;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.Mediator;
using Unity.Entities;
using UnityEngine.UI;

public partial struct OwnerShipLabelImageUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<LabelGOsComponent>();
        state.RequireForUpdate<CanvasGORef>();
        state.RequireForUpdate<OwnerComponent>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<CharacterSpriteDictionary>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (propertyName, ownerID) in SystemAPI.Query<RefRO<NameComponent>, RefRO<OwnerComponent>>().WithChangeFilter<OwnerComponent>())
        {
            var canvasGO = SystemAPI.ManagedAPI.GetSingleton<CanvasGORef>();
            if (canvasGO.CanvasGO == null)
                break;

        foreach (var (playerName, playerID) in SystemAPI.Query<RefRO<NameComponent>, RefRO<PlayerID>>())
        {
            if (ownerID.ValueRO.ID == playerID.ValueRO.Value)
            {
                var labelGOs = SystemAPI.ManagedAPI.GetSingleton<LabelGOsComponent>().GameObjects;
                var imagePanel = labelGOs[propertyName.ValueRO.Value].transform.GetChild(0).GetComponentInChildren<Image>();
                var characterSpriteDictionary = SystemAPI.ManagedAPI.GetSingleton<CharacterSpriteDictionary>();
                UnityEngine.Debug.Log($"[OwnerShipLabelImageUpdateSystem] | Looking up sprite for {playerName.ValueRO.Value}");
                if (characterSpriteDictionary.Value.TryGetValue(playerName.ValueRO.Value, out var test))
                {
                    UnityEngine.Debug.Log($"[OwnerShipLabelImageUpdateSystem] | Found sprite {playerName.ValueRO.Value}");
                }
                else
                {
                    UnityEngine.Debug.Log($"[OwnerShipLabelImageUpdateSystem] | Did not find sprite {playerName.ValueRO.Value}");
                }
                imagePanel.sprite = characterSpriteDictionary.Value[playerName.ValueRO.Value];
                labelGOs[propertyName.ValueRO.Value].SetActive(true);
            }
            if (ownerID.ValueRO.ID == PropertyConstants.Vacant)
            {
                var labelGOs = SystemAPI.ManagedAPI.GetSingleton<LabelGOsComponent>().GameObjects;
                labelGOs[propertyName.ValueRO.Value].SetActive(false);
            }
        }
    }
}
}
