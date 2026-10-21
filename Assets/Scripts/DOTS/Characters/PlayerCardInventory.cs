using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.Characters
{
    [GhostComponent]
    public struct PlayerCardInventory : IComponentData
    {
        [GhostField] public byte Count;
        [GhostField] public byte Card0;
        [GhostField] public byte Card1;
        [GhostField] public byte Card2;
        [GhostField] public byte Card3;
        [GhostField] public bool HasPendingChoice;
        [GhostField] public byte PendingCard;
    }
}
