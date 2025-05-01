using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct PanelPositionTopRight
{
    public float Top { get; set; }
    public float Right { get; set; }
}

public class StatsPanelsPositionsCalculator
{
    public List<float> SmallPanelsPositions { get; private set; }
    public float CurrentPlayerPanelPosition { get; private set; }
    public Vector2 LargePanelPosition { get; private set; }
    private readonly VisualElement _container;

    public StatsPanelsPositionsCalculator(VisualElement container)
    {
        _container = container;
        SmallPanelsPositions = new();
    }

    public void CalculatePanelPosition(VisualElement panel)
    {
        var count = SmallPanelsPositions.Count;
        SmallPanelsPositions.Add(panel.resolvedStyle.width * count);
    }

    public PanelPositionTopRight GetCurrentPlayerPanelPosition(VisualElement panel)
    {
        return new PanelPositionTopRight { Top = 130, Right = GetWidthOfContainer() - panel.resolvedStyle.width };
    }

    private float GetWidthOfContainer()
    {
        return _container.resolvedStyle.width;
    }

    public PanelPositionTopRight GetPanelPosition(int idx)
    {
        return new PanelPositionTopRight { Top = 0, Right = SmallPanelsPositions[idx] };
    }
}
