using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public static class MatchRules
    {
        public static int RoundLimit(int selected) => selected == 10 || selected == 12 ? selected : 8;
        public static int PropertyValue(int price, int housePrice, int houses) =>
            System.Math.Max(0, price) + System.Math.Max(0, housePrice) * System.Math.Max(0, System.Math.Min(5, houses));
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(BankruptcySystem))]
    [UpdateAfter(typeof(ChangeTurnSystem))]
    public partial struct MatchCompletionSystem : ISystem
    {
        bool finalized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentRound>();
            state.RequireForUpdate<BankruptPlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            var previous = game;
            if (!game.AllPlacesInstantiated) return;
            if (game.State != GameState.GameOver && BankruptcyRules.HasDebt(state.EntityManager)) return;
            if (game.RoundLimit == 0) game.RoundLimit = MatchRules.RoundLimit(NetworkRequests.MatchRoundLimit);
            game.CompletedRounds = SystemAPI.GetSingleton<CurrentRound>().Value;
            bool finish = game.State != GameState.GameOver && game.CompletedRounds >= game.RoundLimit;
            if (!finish && (game.State != GameState.GameOver || finalized))
            {
                if (previous.RoundLimit != game.RoundLimit || previous.CompletedRounds != game.CompletedRounds)
                    SystemAPI.SetSingleton(game);
                return;
            }
            int best = int.MinValue, winner = 0;
            bool tied = false;
            foreach (var (money, result, owner, player) in SystemAPI.Query<RefRO<GhostMoneyComponet>, RefRW<BankruptPlayer>, RefRO<GhostOwner>>().WithEntityAccess())
            {
                int cashWorth = result.ValueRO.Value ? 0 : System.Math.Max(0, money.ValueRO.Value);
                int propertyWorth = 0;
                int buildingWorth = 0;
                if (!result.ValueRO.Value)
                    foreach (var (propertyOwner, price, property) in SystemAPI.Query<RefRO<OwnerByEntityComponent>, RefRO<GhostPriceComponent>>().WithEntityAccess())
                    {
                        if (propertyOwner.ValueRO.Entity != player) continue;
                        int houses = SystemAPI.HasComponent<HouseCount>(property) ? SystemAPI.GetComponent<HouseCount>(property).Value : 0;
                        int cost = SystemAPI.HasComponent<HousePriceComponent>(property) ? SystemAPI.GetComponent<HousePriceComponent>(property).Value : 0;
                        propertyWorth += System.Math.Max(0, price.ValueRO.Value);
                        buildingWorth += System.Math.Max(0, cost) * System.Math.Max(0, System.Math.Min(5, houses));
                    }
                int worth = cashWorth + propertyWorth + buildingWorth;
                result.ValueRW.FinalCashWorth = cashWorth;
                result.ValueRW.FinalPropertyWorth = propertyWorth;
                result.ValueRW.FinalBuildingWorth = buildingWorth;
                result.ValueRW.FinalNetWorth = worth;
                if (result.ValueRO.Value) continue;
                if (worth > best) { best = worth; winner = owner.ValueRO.NetworkId; tied = false; }
                else if (worth == best) tied = true;
            }
            if (finish)
            {
                game.WinnerNetworkId = tied ? 0 : winner;
                game.EndedByRoundLimit = true;
                game.State = GameState.GameOver;
            }
            finalized = true;
            SystemAPI.SetSingleton(game);
        }
    }
}
