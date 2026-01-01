using System.Collections.Generic;
using UnityEngine.UIElements;

public struct OffsetFromTopRight
{
    public float Top { get; set; }
    public float Right { get; set; }
}

public class StatsPanelsPositionsCalculator
{
    private readonly List<VisualElement> _panels; 
    private readonly List<OffsetFromTopRight> _offsetFromTopRightPositions; 
    private readonly VisualElement _container;
    private float ContainerWidth => _container == null ? 0 : _container.resolvedStyle.width;

    public StatsPanelsPositionsCalculator(VisualElement container)
    {
        _container = container;
        _panels = new();
        _offsetFromTopRightPositions = new();
    }

    public void AddPanel(VisualElement panel)
    {
        _panels.Add(panel);
    }
    
    public void CalculatePositions()
    {
        var numOfPanels = _panels.Count;
        for (int i = 0; i < numOfPanels; i++)
        {
            var width = _panels[i].resolvedStyle.width;
            var offsetFromRight = width * i;
            _offsetFromTopRightPositions.Add(new OffsetFromTopRight { Top = 0, Right = offsetFromRight });
        }
    }

    public OffsetFromTopRight GetCurrentPlayerPanelPosition(VisualElement panel)
    {
        UnityEngine.Debug.Log($"[StatsPanelsPositionsCalculator] | positon = 130, right  = {ContainerWidth - panel.resolvedStyle.width} ");
        UnityEngine.Debug.Log($"[StatsPanelsPositionsCalculator] | right  container width= {ContainerWidth} ");
        UnityEngine.Debug.Log($"[StatsPanelsPositionsCalculator] | right  panel width= {panel.resolvedStyle.width} ");
        return new OffsetFromTopRight { Top = 130, Right = ContainerWidth - panel.resolvedStyle.width };
    }

    public OffsetFromTopRight GetPanelPosition(int idx)
    {
        UnityEngine.Debug.Log($"[StatsPanelsPositionsCalculator] | regular panel top = {_offsetFromTopRightPositions[idx].Top} right {_offsetFromTopRightPositions[idx].Right}   ");
        return _offsetFromTopRightPositions[idx]; 
    }
}
