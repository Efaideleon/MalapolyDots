using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public enum SpaceActionButtonsEnum
    {
        BuyHouse,
        BuyHotel,
        BuyProperty,
        PayRent
    }

    public struct ButtonData 
    {
        public SpaceActionButtonsEnum Type { get; set;}
        public string ContainerUXMLClassName { get; set;}
        public string ButtonUXMLClassName { get; set;}
    }

    public static class SpaceActionsPanelData
    {
        public const string PanelUXMLClassName = "SpaceActions";
        public const string AnimationFromBottomUXMLClassName = "animation-translation-from-bottom";
        public static readonly ButtonData[] ButtonData = new ButtonData[]
        {
            new() 
            {
                Type = SpaceActionButtonsEnum.BuyHouse,
                ContainerUXMLClassName = "buy-houses-btn-container",
                ButtonUXMLClassName = "buy-house-button"
            },
            new() 
            {
                Type = SpaceActionButtonsEnum.BuyHotel,
                ContainerUXMLClassName = "buy-hotel-btn-container",
                ButtonUXMLClassName = null
            },
            new() 
            {
                Type = SpaceActionButtonsEnum.BuyProperty,
                ContainerUXMLClassName = "buy-property-btn-container",
                ButtonUXMLClassName = "buy-property-button"
            },
            new() 
            {
                Type = SpaceActionButtonsEnum.PayRent,
                ContainerUXMLClassName = null,
                ButtonUXMLClassName = "pay-rent"
            }
        };
    }

    public struct ButtonElement
    {
        public VisualElement Container { get; set; }
        public Button Button { get; set; }
    }

    public class SpaceActionsPanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public readonly Dictionary<SpaceActionButtonsEnum, ButtonElement> ButtonSet = new();

        public SpaceActionsPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>(SpaceActionsPanelData.PanelUXMLClassName);

            foreach (var buttonData in SpaceActionsPanelData.ButtonData)
            {
                ButtonSet.Add(buttonData.Type, new ButtonElement
                {
                    Button = buttonData.ButtonUXMLClassName != null ? Panel.Q<Button>(buttonData.ButtonUXMLClassName) : null,
                    Container = buttonData.ContainerUXMLClassName != null ? Panel.Q<VisualElement>(buttonData.ContainerUXMLClassName) : null
                });
            }
        }

        public void Show()
        {
            AddClassAnimation(); 
            ToggleButtonsState(true);
        }

        public void Hide()
        {
            RemoveClassAnimation(); 
            ToggleButtonsState(false);
        }

        private void AddClassAnimation()
        {
            foreach (var kvp in ButtonSet.Values)
                    kvp.Container?.AddToClassList(SpaceActionsPanelData.AnimationFromBottomUXMLClassName);
            ButtonSet[SpaceActionButtonsEnum.PayRent].Button.AddToClassList(SpaceActionsPanelData.AnimationFromBottomUXMLClassName);
        }

        private void RemoveClassAnimation()
        {
            foreach (var kvp in ButtonSet.Values)
                    kvp.Container?.RemoveFromClassList(SpaceActionsPanelData.AnimationFromBottomUXMLClassName);
            ButtonSet[SpaceActionButtonsEnum.PayRent].Button.RemoveFromClassList(SpaceActionsPanelData.AnimationFromBottomUXMLClassName);
        }

        private void ToggleButtonsState(bool state)
        {
            foreach (var kvp in ButtonSet.Values)
                kvp.Button?.SetEnabled(state);
        }
    }
}
