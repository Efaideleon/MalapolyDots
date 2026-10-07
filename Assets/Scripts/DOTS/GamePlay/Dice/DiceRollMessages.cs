using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public struct DiceRollResultRpc : IRpcCommand
    {
        public int First, Second;
        public uint Sequence;
    }

    public struct DiceSettledRpc : IRpcCommand { public uint Sequence; }

    public struct PendingDiceRoll : IComponentData
    {
        public Entity Player;
        public int First, Second;
        public uint Sequence;
        public double EarliestMoveTime, Timeout;
        public bool Active, Settled;
    }

    public static class DiceRollValues
    {
        public const float FloorRollSeconds = 1.65f;
        public const float FloorPauseSeconds = .3f;
        public const float PullSeconds = .8f;
        public const float AnimationSeconds = FloorRollSeconds + FloorPauseSeconds + PullSeconds;
        public const float ResultHoldSeconds = 1.4f;

        // The existing editor override chooses a total. Preserve it using two legal d6 faces.
        public static int2 FromTotal(int total, ref Random random)
        {
            total = math.clamp(total, 2, 12);
            int first = random.NextInt(math.max(1, total - 6), math.min(6, total - 1) + 1);
            return new int2(first, total - first);
        }
    }
}
