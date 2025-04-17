using UnityEngine.UIElements;

public class PayRentPanel
{
    public VisualElement Panel { get; private set; }
    public Button AcceptButton { get; private set; }
    public Label RentAmountLabel { get; private set; }

    public PayRentPanel(VisualElement parent) 
    { 
        Panel = parent.Q<VisualElement>("PayRentPanel");
        RentAmountLabel = Panel.Q<Label>("pay-rent-amount-label");
        AcceptButton = Panel.Q<Button>("pay-rent-ok-button");
        //Hide();
    }

    public void UpdateRentAmountLabel(string amount) => RentAmountLabel.text = amount;
 
    public void Show() => Panel.style.display = DisplayStyle.Flex;
    public void Hide() => Panel.style.display = DisplayStyle.None;
}
