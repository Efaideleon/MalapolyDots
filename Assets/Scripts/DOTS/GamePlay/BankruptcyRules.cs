using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public static class BankruptcyRules
    {
        public static bool HasDebt(EntityManager manager)
        {
            using var query = manager.CreateEntityQuery(typeof(GhostMoneyComponet), typeof(BankruptPlayer));
            using var players = query.ToEntityArray(Allocator.Temp);
            foreach (var player in players)
                if (!manager.GetComponentData<BankruptPlayer>(player).Value &&
                    manager.GetComponentData<GhostMoneyComponet>(player).Value < 0) return true;
            return false;
        }
    }
}
