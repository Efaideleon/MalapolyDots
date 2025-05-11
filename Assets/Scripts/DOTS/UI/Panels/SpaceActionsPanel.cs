using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class ButtonEvent : IVisibilityEvent
    {
        private readonly Action _action;
        public ButtonEvent(Action action)
        {
            _action = action;
        }
        public void Execute()
        {
            _action();
        }
    }

    public class SpaceActionsPanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public VisualElement BuyHouseButtonContainer { get; private set; }
        public VisualElement BuyHotelButtonContainer { get; private set; }
        public VisualElement BuyPropertyButtonContainer { get; private set; }
        public Button BuyHouseButton { get; private set; }
        public Button BuyPropertyButton { get; private set; }
        public Button PayRentButton { get; private set; }
        private readonly Queue<IVisibilityEvent> VisibilityEvents = new();

        private bool isPlaying = false;

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
            //Panel.style.display = DisplayStyle.None;
            SetupCallbacks();
        }

        private readonly StylePropertyName styleTranslate = new("translate");
        private void SetupCallbacks()
        {
            PayRentButton.RegisterCallback<TransitionEndEvent>(e => 
            {
                if (e.stylePropertyNames.Contains(styleTranslate)) 
                {
                    UnityEngine.Debug.Log($"TransitionEnded");
                    if (VisibilityEvents.Count > 0)
                    {
                        UnityEngine.Debug.Log($"Dequeueing event {VisibilityEvents.Count}");
                        VisibilityEvents.Dequeue().Execute();
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"setting isPLaying to false event {VisibilityEvents.Count}");
                        isPlaying = false;
                    }
                }
            });
        }

        private void ShowButtons() 
        {
            AddClassAnimation(); 
            EnableButtons();
        }
        private void HideButtons() 
        {
            RemoveClassAnimation(); 
            DisableButtons();
        }

        public void Show()
        {
            UnityEngine.Debug.Log($"Show isPlaying: {isPlaying}");
            if (!isPlaying)
            {
                isPlaying = true;
                UnityEngine.Debug.Log($"Just Showing {VisibilityEvents.Count}");
                Panel.schedule.Execute(_ =>
                {
                    ShowButtons();
                }).StartingIn(1);
            }
            else
            {
                UnityEngine.Debug.Log($"Enqueue Show event {VisibilityEvents.Count}");
                VisibilityEvents.Enqueue(new ButtonEvent(ShowButtons));
            }
        }

        public void Hide()
        {
            UnityEngine.Debug.Log($"Hide isPlaying: {isPlaying}");
            if (!isPlaying)
            {
                isPlaying = true;
                UnityEngine.Debug.Log($"Just Hide {VisibilityEvents.Count}");
                Panel.schedule.Execute(_ =>
                {
                    HideButtons();
                }).StartingIn(1);
            }
            else
            {
                UnityEngine.Debug.Log($"Enqueue Hide event {VisibilityEvents.Count}");
                VisibilityEvents.Enqueue(new ButtonEvent(HideButtons));
            }
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
    }
}
