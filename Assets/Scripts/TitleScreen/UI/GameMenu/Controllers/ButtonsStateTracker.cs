using System.Collections.Generic;
using System.Linq;

public class ButtonsStateTracker
{
    private readonly Dictionary<Character, ButtonState> _buttonStateTracker;

    public ButtonsStateTracker()
    {
        _buttonStateTracker = new();
    }

    public void RegisterButton(Character buttonType)
    {
        _buttonStateTracker.TryAdd(buttonType, ButtonState.Default);
    }

    public void UpdateState(Character buttonType,  ButtonState state)
    {
        _buttonStateTracker[buttonType] = state; 
    }

    public bool TryGetState(Character buttonType, out ButtonState state)
    {
        if (_buttonStateTracker.TryGetValue(buttonType, out var outState))
        {
            state = outState;
            return true;
        }
        state = outState;
        return false;
    }

    public void ResetAvaiblabeButtonsState()
    {
        for (int i = 0; i < _buttonStateTracker.Count; i++)
        {
            var key = _buttonStateTracker.Keys.ElementAt(i);
            if (_buttonStateTracker[key] != ButtonState.Unavailable)
                _buttonStateTracker[key] = ButtonState.Default;
        }
    }
}
