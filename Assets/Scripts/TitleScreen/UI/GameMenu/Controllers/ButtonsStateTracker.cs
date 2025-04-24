using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public class ButtonsStateTracker
{
    private readonly Dictionary<FixedString64Bytes, CharacterButtonState> _buttonStateTracker;

    public ButtonsStateTracker()
    {
        _buttonStateTracker = new();
    }

    public void RegisterButton(FixedString64Bytes name)
    {
        _buttonStateTracker.TryAdd(name, CharacterButtonState.Default);
    }

    public void UpdateState(FixedString64Bytes name,  CharacterButtonState state)
    {
        _buttonStateTracker[name] = state; 
    }

    public bool TryGetState(FixedString64Bytes name, out CharacterButtonState state)
    {
        if (_buttonStateTracker.TryGetValue(name, out var outState))
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
            if (_buttonStateTracker[key] != CharacterButtonState.Unavailable)
                _buttonStateTracker[key] = CharacterButtonState.Default;
        }
    }
}
