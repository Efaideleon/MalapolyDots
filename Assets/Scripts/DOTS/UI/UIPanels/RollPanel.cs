using UnityEngine.UIElements;
using Assets.Scripts.DOTS.UI.UIPanels;
using System;

public class RollPanel : Panel
{
    public Button RollButton { get; private set; }
    public Label RollLabel { get; private set; }

    public Action OnRollButton;

    public RollPanel(VisualElement parent) : base(parent.Q<VisualElement>("RollPanel"))
    {
        RollButton = Root.Q<Button>("roll-button");
        RollLabel = Root.Q<Label>("roll-amount-label");
        Hide();
    }

    public void AddActionToRollButton(Action action)
    {
        OnRollButton = action;
        RollButton.clickable.clicked += OnRollButton;
    }

    private void UnsubscribeRollButton()
    {
        RollButton.clickable.clicked -= OnRollButton;
    }

    public void UpdateRollLabel(string text)
    {
        RollLabel.text = text;
    }

    public override void Show()
    {
        base.Show();
        UpdateRollLabel("0");
        ShowRollButton();
    }

    public void HideRollButton() => RollButton.style.display = DisplayStyle.None;
    public void ShowRollButton() => RollButton.style.display = DisplayStyle.Flex;

    public override void Dispose() => UnsubscribeRollButton();
}
