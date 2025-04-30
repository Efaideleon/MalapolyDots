using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StatsPanelsPositionsCalculator
{
    public List<float> SmallPanelsPositions { get; private set; }
    public Vector2 LargePanelPosition { get; private set; }
    private float _widthOfContainer; 
    private readonly VisualElement _container;

    public StatsPanelsPositionsCalculator(VisualElement container) 
    {
        _container = container;
    
        SmallPanelsPositions = new ();
    }

    public void CalculatePanelPosition(VisualElement panel)
    {
        var count = SmallPanelsPositions.Count;
        SmallPanelsPositions.Add(panel.resolvedStyle.width * count);
    }

    public float GetPanelPosition(int idx)
    {
        return SmallPanelsPositions[idx];
    }

    private void CalculateWidthOfContainer()
    {
        _widthOfContainer = _container.resolvedStyle.width;
    }
}
