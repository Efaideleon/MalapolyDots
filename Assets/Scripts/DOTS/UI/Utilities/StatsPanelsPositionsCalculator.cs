using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace DOTS.UI.Utilities
{
    public struct OffsetFromTopRight
    {
        public OffsetFromTopRight(float top, float right)
        {
            Top = top;
            Right = right;
        }

        public float Top { get; set; }
        public float Right { get; set; }
    }

    public class StatsPanelsPositionsCalculator
    {
        private readonly List<OffsetFromTopRight> _positions;
        private float ContainerWidth => _container.resolvedStyle.width;
        private readonly VisualElement _container;
        private float _panelWidth;

        public StatsPanelsPositionsCalculator(VisualElement container)
        {
            _positions = new();
            _container = container;
        }

        // this can only be called when the container's width is resolved.
        public void CalculatePositions(float panelWidth)
        {
            _panelWidth = panelWidth;
        }

        public OffsetFromTopRight GetPosition(int idx)
        {
            return _positions[idx];
        }

        public OffsetFromTopRight ShiftLeft(int amount)
        {
            return new OffsetFromTopRight { Top = 0, Right = -_panelWidth * amount };
        }

        public OffsetFromTopRight GetOffsetScreenRightPosition()
        {
            return new OffsetFromTopRight(350, 0);
        }

        public OffsetFromTopRight GetHighlightPosition()
        {
            return new OffsetFromTopRight(130, -(ContainerWidth - _panelWidth));
        }
    }
}
