using Unity.Collections;
using Unity.Entities;

// This System will calculate if the player has any properties where they can buy a house
// If they have all the properties of the same color then mark a flag that keeps track if a house can be bought
// on the property or not.
public partial struct MonopolyTrackerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<OwnerComponent>();
        state.RequireForUpdate<ColorCodeComponent>();
        state.RequireForUpdate<MonopolyFlagComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        bool ownerChanged = false;
        // I wonder how many this this loop iterates?
        // TODO: I wonder if I can just detect a change and then continue instead of looping here 20 times
        foreach ( var _ in SystemAPI.Query<RefRO<OwnerComponent>>().WithChangeFilter<OwnerComponent>())
        {
            ownerChanged = true;
        }

        if (ownerChanged)
        {
            // Create a NativeHashMap to check if someone has a monopoly on the color
            // TODO: Maybe I Should store this in an entity so that I don't have to rebuild it every time
            NativeHashMap<int, NativeList<int>> monopolyTracker = new (9, Allocator.Temp);
            monopolyTracker.TryAdd((int)PropertyColor.Brown, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.LightBlue, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Purple, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Orange, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Red, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Yellow, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Green, new NativeList<int>(Allocator.Temp));
            monopolyTracker.TryAdd((int)PropertyColor.Blue, new NativeList<int>(Allocator.Temp));

            foreach (var (owner, color)  in SystemAPI.Query<RefRO<OwnerComponent>, RefRO<ColorCodeComponent>>())
            {
                if (color.ValueRO.Value != PropertyColor.White)
                {
                    monopolyTracker[(int)color.ValueRO.Value].Add(owner.ValueRO.ID);
                }
            }

            // TODO: Remove the name later
            foreach (var (color, monopolyFlag, _) in 
                    SystemAPI.Query<
                        RefRO<ColorCodeComponent>,
                        RefRW<MonopolyFlagComponent>,
                        RefRO<PropertySpaceTag>
                    >())
            {
                switch(color.ValueRO.Value)
                {
                    case PropertyColor.Brown:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Brown, monopolyTracker);
                        break;
                    case PropertyColor.LightBlue:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.LightBlue, monopolyTracker);
                        break;
                    case PropertyColor.Purple:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Purple, monopolyTracker);
                        break;
                    case PropertyColor.Orange:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Orange, monopolyTracker);
                        break;
                    case PropertyColor.Red:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Red, monopolyTracker);
                        break;
                    case PropertyColor.Yellow:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Yellow, monopolyTracker);
                        break;
                    case PropertyColor.Green:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Green, monopolyTracker);
                        break;
                    case PropertyColor.Blue:
                        monopolyFlag.ValueRW.Value = IsMonopoly(PropertyColor.Blue, monopolyTracker);
                        break;
                }
            }
        }
    }

    // Calcuate if there is a monopoly over a given color
    private static bool IsMonopoly(
            in PropertyColor color,
            NativeHashMap<int, NativeList<int>> monopolyTracker) 
    {
        bool isMonopoly = false;
        if (monopolyTracker[(int)color].Length > 0)
        {
            var firstOwner = monopolyTracker[(int)color][0];
            if (firstOwner == PropertyConstants.Vacant)
                return false;

            foreach (var currOwner in monopolyTracker[(int)color])
            {
                if (firstOwner != currOwner)
                    return false;
                isMonopoly = true;
            }
        }
        return isMonopoly;
    }
}

