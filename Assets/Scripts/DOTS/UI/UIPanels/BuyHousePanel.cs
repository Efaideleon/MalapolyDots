using UnityEngine.UIElements;

public class BuyHousePanel 
{
    public VisualElement Root { get; private set; }
    public Label PriceLabel { get; private set; }
    public Label TitleLabel { get; private set; }
    public Button BuyButton { get; private set; }
    public Button DeclineButton { get; private set; }
    
    public BuyHousePanel(VisualElement Root) 
    {
        if (Root != null)
        {
            this.Root = Root;
            TitleLabel = Root.Q<Label>("upgrade-house-title-label");
            PriceLabel = Root.Q<Label>("upgrade-house-price-label");
            DeclineButton = Root.Q<Button>("upgrade-house-decline-button");
            BuyButton = Root.Q<Button>("upgrade-house-accept-button");
            Hide();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Root for BuyHousePanel is null");
        }
    }

    public virtual void Show() => Root.style.display = DisplayStyle.Flex;
    public virtual void Hide() => Root.style.display = DisplayStyle.None;
}
