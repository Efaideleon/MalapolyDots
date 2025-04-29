using System;

public class SelectionHighlighter<T>
{
    private T _previous;
    private readonly Action<T> _enable;
    private readonly Action<T> _disable;

    public SelectionHighlighter(Action<T> enable, Action<T> disable)
    {
        _enable = enable;
        _disable = disable;
    }

    public void Select(T current)
    {
        if (_previous != null)
            _disable(_previous);

        _enable(current);
        _previous = current;
    }
}
