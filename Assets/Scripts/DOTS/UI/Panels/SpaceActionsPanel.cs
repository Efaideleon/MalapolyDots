using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class SpaceActionsPanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public VisualElement BuyHouseButtonContainer { get; private set; }
        public VisualElement BuyHotelButtonContainer { get; private set; }
        public VisualElement BuyPropertyButtonContainer { get; private set; }
        public Button BuyHouseButton { get; private set; }
        public Button BuyPropertyButton { get; private set; }
        public Button PayRentButton { get; private set; }

        private bool inTransition = false;

        public SpaceActionsPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>("SpaceActions");

            BuyHouseButtonContainer = Panel.Q<VisualElement>("buy-houses-btn-container");
            BuyHotelButtonContainer = Panel.Q<VisualElement>("buy-hotel-btn-container");
            BuyPropertyButtonContainer = Panel.Q<VisualElement>("buy-property-btn-container");
            PayRentButton = Panel.Q<Button>("pay-rent");
            BuyHouseButton = Panel.Q<Button>("buy-house-button");
            if (BuyHouseButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyButton is null");
            }
            BuyPropertyButton = Panel.Q<Button>("buy-property-button");
            if (BuyPropertyButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyPropertyButton is null");
            }
            Panel.style.display = DisplayStyle.None;
            SetupCallbacks();
        }

        private void SetupCallbacks()
        {
            BuyHouseButtonContainer.RegisterCallback<TransitionEndEvent>(e => 
            {
                inTransition = false;
            });
            BuyHotelButtonContainer.RegisterCallback<TransitionEndEvent>(e =>
            {
                inTransition = false;
            });
            BuyPropertyButtonContainer.RegisterCallback<TransitionEndEvent>(e => 
            {
                inTransition = false;
            });
            BuyPropertyButtonContainer.RegisterCallback<TransitionEndEvent>(e =>
            {
                inTransition = false;
            });
        }

        public void Show()
        {
            UnityEngine.Debug.Log($"In show inTransition: {inTransition}");
            if (inTransition == false)
            {
                UnityEngine.Debug.Log("Showing not in transition");
                Panel.style.display = DisplayStyle.Flex;
                AddClassAnimation();
                inTransition = true;
                Panel.schedule.Execute(() =>
                {
                    EnableButtons();
                }).StartingIn(400);
            }
            else 
            {
                Panel.schedule.Execute(() =>
                {
                    UnityEngine.Debug.Log("Showing inTransition");
                    Panel.style.display = DisplayStyle.Flex;
                    AddClassAnimation();
                }).StartingIn(500);
                Panel.schedule.Execute(() =>
                {
                    EnableButtons();
                }).StartingIn(900);
            }
            UnityEngine.Debug.Log("Showing the SpaceActions panel");
        }

        private void AddClassAnimation()
        {
            BuyHouseButtonContainer.AddToClassList("animation-translation-from-bottom");
            BuyHotelButtonContainer.AddToClassList("animation-translation-from-bottom");
            BuyPropertyButtonContainer.AddToClassList("animation-translation-from-bottom");
            PayRentButton.AddToClassList("animation-translation-from-bottom");
        }

        private void RemoveClassAnimation()
        {
            BuyHouseButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            BuyHotelButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            BuyPropertyButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            PayRentButton.RemoveFromClassList("animation-translation-from-bottom");
        }

        private void EnableButtons()
        {
            BuyHouseButton.SetEnabled(true);
            BuyPropertyButton.SetEnabled(true);
            PayRentButton.SetEnabled(true);
        }

        private void DisableButtons()
        {
            BuyHouseButton.SetEnabled(false);
            BuyPropertyButton.SetEnabled(false);
            PayRentButton.SetEnabled(false);
        }

        public void Hide()
        {
            DisableButtons();
            if (inTransition == false)
            {
                RemoveClassAnimation();
                Panel.schedule.Execute(() =>
                {
                    // Panel.style.display = DisplayStyle.None;
                }).StartingIn(400);
                inTransition = true;
            }
            else
            {
                Panel.schedule.Execute(() =>
                {
                    RemoveClassAnimation();
                }).StartingIn(500);

                Panel.schedule.Execute(() =>
                {
                    // Panel.style.display = DisplayStyle.None;
                }).StartingIn(800);
            }
            UnityEngine.Debug.Log("Hiding the SpaceActions panel");
        }
    }
}
