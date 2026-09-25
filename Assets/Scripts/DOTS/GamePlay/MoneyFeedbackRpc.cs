using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public enum MoneyChangeReason : byte { Purchase, Rent, Tax, Card, PassGo, Trade, Building }

    // Both sides travel together so every client presents the initiator before the recipient.
    public struct MoneyFeedbackRpc : IRpcCommand
    {
        public int FirstPlayerId, FirstDelta, SecondPlayerId, SecondDelta;
        public MoneyChangeReason Reason;
        public int PropertyId;
    }

    public static class MoneyFeedback
    {
        public static void Send(EntityCommandBuffer commands, EntityManager entities,
            Entity first, int firstDelta, MoneyChangeReason reason, Entity second = default, int secondDelta = 0,
            int propertyId = -1)
        {
            if ((firstDelta == 0 && secondDelta == 0) || !entities.HasComponent<GhostOwner>(first)) return;
            var message = new MoneyFeedbackRpc
            {
                FirstPlayerId = entities.GetComponentData<GhostOwner>(first).NetworkId,
                FirstDelta = firstDelta,
                Reason = reason,
                PropertyId = propertyId
            };
            if (second != Entity.Null && entities.HasComponent<GhostOwner>(second))
            {
                message.SecondPlayerId = entities.GetComponentData<GhostOwner>(second).NetworkId;
                message.SecondDelta = secondDelta;
            }
            var rpc = commands.CreateEntity();
            commands.AddComponent(rpc, message);
            commands.AddComponent<SendRpcCommandRequest>(rpc);
        }
    }
}
