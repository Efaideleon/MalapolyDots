using Assets.Scripts.DOTS.GamePlay;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(PurchasePropertySystem))]
    [UpdateBefore(typeof(BuyHouseSystem))]
    public partial struct MonopolyTrackerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PropertySpaceTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Do not award a group while the board is still being instantiated.
            if (SystemAPI.TryGetSingleton<GameStateComponent>(out var gameState) && !gameState.AllPlacesInstantiated)
                return;

            var owners = new NativeArray<int>((int)PropertyColor.Blue + 1, Allocator.Temp);
            var counts = new NativeArray<int>(owners.Length, Allocator.Temp);
            var mixed = new NativeArray<bool>(owners.Length, Allocator.Temp);
            foreach (var (owner, color) in SystemAPI.Query<RefRO<OwnerComponent>, RefRO<ColorCodeComponent>>().WithAll<PropertySpaceTag>())
            {
                int index = (int)color.ValueRO.Value;
                if (index < (int)PropertyColor.Brown || index >= owners.Length) continue;
                if (counts[index] == 0)
                {
                    owners[index] = owner.ValueRO.ID;
                }
                else if (owners[index] != owner.ValueRO.ID)
                {
                    mixed[index] = true;
                }
                counts[index]++;
            }

            foreach (var (color, monopoly) in SystemAPI.Query<RefRO<ColorCodeComponent>, RefRW<MonopolyFlagComponent>>().WithAll<PropertySpaceTag>())
            {
                int index = (int)color.ValueRO.Value;
                bool complete = index >= (int)PropertyColor.Brown && index < owners.Length &&
                    counts[index] >= 2 && !mixed[index] && owners[index] != PropertyConstants.Vacant;

                if (monopoly.ValueRO.Value != complete)
                {
                    monopoly.ValueRW.Value = complete;
                }
            }
            owners.Dispose();
            counts.Dispose();
            mixed.Dispose();
        }
    }
}
