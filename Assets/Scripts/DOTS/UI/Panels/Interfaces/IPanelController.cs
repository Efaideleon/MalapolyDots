public interface IPanelController
{
    public void ShowPanel();
    public void HidePanel();
}

public interface IPanelControllerSimple
{
    public void Show();
    public void Hide();
}

public interface IPanelControllerNew<T> : IPanelControllerSimple
{
    public void Update(T data);
    public void Dispose();
    //public void SetEventBufferQuery();
}
