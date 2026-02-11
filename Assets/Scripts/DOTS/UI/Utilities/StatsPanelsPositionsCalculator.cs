using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace DOTS.UI.Utilities
{
    public struct OffsetFromTopRight
    {
        public float Top { get; set; }
        public float Right { get; set; }
    }

    public class StatsPanelsPositionsCalculator
    {
        private readonly List<OffsetFromTopRight> _positions;
        private float ContainerWidth => _container.resolvedStyle.width;
        private readonly VisualElement _container;

        public StatsPanelsPositionsCalculator(VisualElement container)
        {
            _positions = new();
            _container = container;
        }

        // this can only be called when the container's width is resolved.
        public void CalculatePositions(float panelWidth, int numOfPanels)
        {
            LinkedList<OffsetFromTopRight> linkedList = new();

            // Working from right to left.
            for (int i = 0; i < numOfPanels - 1; i++)
            {
                var offsetFromRight = panelWidth * i;
                var panelPositon = new OffsetFromTopRight { Top = 0, Right = offsetFromRight };
                linkedList.AddFirst(panelPositon);
            }

            var highlightPanelPosition = new OffsetFromTopRight { Top = 130, Right = ContainerWidth - panelWidth };;
            linkedList.AddFirst(highlightPanelPosition);

            _positions.Clear();
            _positions.AddRange(linkedList.ToList());
        }

        public OffsetFromTopRight GetPosition(int idx)
        {
            return _positions[idx];
        }
    }
}
