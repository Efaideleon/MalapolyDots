using System.Collections.Generic;

public class ButtonsStateTracker
{
    private readonly Dictionary<Character, AvailableState> _buttonStateTracker;

    public ButtonsStateTracker()
    {
        _buttonStateTracker = new();
    }

    public void RegisterButton(Character buttonType)
    {
        _buttonStateTracker.TryAdd(buttonType, AvailableState.Available);
    }

    public void UpdateState(Character buttonType, AvailableState state)
    {
        _buttonStateTracker[buttonType] = state; 
    }

    public bool TryGetState(Character buttonType, out AvailableState state)
    {
        if (_buttonStateTracker.TryGetValue(buttonType, out var outState))
        {
            state = outState;
            return true;
        }
        state = outState;
        return false;
    }
}
