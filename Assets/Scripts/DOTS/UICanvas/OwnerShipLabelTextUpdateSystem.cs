using DOTS.Characters;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using TMPro;
using Unity.Entities;

public partial struct OwnerShipLabelTextUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<LabelGOsComponent>();
        state.RequireForUpdate<CanvasGORef>();
        state.RequireForUpdate<OwnerComponent>();
        state.RequireForUpdate<NameComponent>();
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
                    var text = labelGOs[propertyName.ValueRO.Value].GetComponentInChildren<TextMeshProUGUI>();
                    text.text = $"Owner: {playerName.ValueRO.Value}";
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
