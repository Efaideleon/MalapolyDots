using System;
using System.Collections.Generic;
using Assets.Scripts.DOTS.DataComponents;
using TitleScreen.NetworkUI.Systems;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Panels
{
    public enum ButtonState
    {
        Default,
        Choosing,
        Unavailable
    }

    public class CharacterSelectPanel : NetworkPanelBase
    {
        private static readonly Dictionary<ButtonState, string> _stateClasses = new()
        {
            { ButtonState.Default, "char-not-picked-btn-container" },
            { ButtonState.Unavailable, "char-disabled-btn-container" },
            { ButtonState.Choosing, "char-picked-btn-container" },
        };

        private readonly Button AvocadoButton;
        private readonly Button BirdButton;
        private readonly Button CoinButton;
        private readonly Button LiraButton;
        private readonly Button CoffeeButton;
        private readonly Button TuctucButton;
        private readonly Button ConfirmButton;

        public CharacterSelectPanel(VisualElement root, Queue<UIRequest> requests) : base(root, requests)
        {
            AvocadoButton = root.Query<Button>("character-one-button");
            BirdButton = root.Query<Button>("character-two-button");
            CoinButton = root.Query<Button>("character-three-button");
            LiraButton = root.Query<Button>("character-four-button");
            CoffeeButton = root.Query<Button>("character-five-button");
            TuctucButton = root.Query<Button>("character-six-button");
            ConfirmButton = root.Query<Button>("character-confirm-button");
        }

        public override void Initialize()
        {
            SubscribeEvents();
        }
        public override void Dispose()
        {
            UnSubscribeEvents();
        }

        private void SubscribeEvents()
        {
            AvocadoButton.RegisterCallback<PointerDownEvent>(SetAvocadoChoosingCallback);
            BirdButton.RegisterCallback<PointerDownEvent>(SetBirdChoosingCallback);
            CoinButton.RegisterCallback<PointerDownEvent>(SetCoinChoosingCallback);
            LiraButton.RegisterCallback<PointerDownEvent>(SetLiraChoosingCallback);
            CoffeeButton.RegisterCallback<PointerDownEvent>(SetCoffeeChoosingCallback);
            TuctucButton.RegisterCallback<PointerDownEvent>(SetTuctucChoosingCallback);

            AvocadoButton.clickable.clicked += HandleAvocadoButton;
            BirdButton.clickable.clicked += HandleBirdButton;
            CoinButton.clickable.clicked += HandleCoinButton;
            LiraButton.clickable.clicked += HandleLiraButton;
            CoffeeButton.clickable.clicked += HandleCoffeeButton;
            TuctucButton.clickable.clicked += HandleTuctucButton;
            ConfirmButton.clickable.clicked += HandleConfirmButton;
        }

        private void SetAvocadoChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, AvocadoButton);
        private void SetBirdChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, BirdButton);
        private void SetCoinChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, CoinButton);
        private void SetLiraChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, LiraButton);
        private void SetCoffeeChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, CoffeeButton);
        private void SetTuctucChoosingCallback(PointerDownEvent evt) => SetButtonColor(ButtonState.Choosing, TuctucButton);

        private void UnSubscribeEvents()
        {
            AvocadoButton.UnregisterCallback<PointerDownEvent>(SetAvocadoChoosingCallback);
            BirdButton.UnregisterCallback<PointerDownEvent>(SetBirdChoosingCallback);
            CoinButton.UnregisterCallback<PointerDownEvent>(SetCoinChoosingCallback);
            LiraButton.UnregisterCallback<PointerDownEvent>(SetLiraChoosingCallback);
            CoffeeButton.UnregisterCallback<PointerDownEvent>(SetCoffeeChoosingCallback);
            TuctucButton.UnregisterCallback<PointerDownEvent>(SetTuctucChoosingCallback);

            AvocadoButton.clickable.clicked -= HandleAvocadoButton;
            BirdButton.clickable.clicked -= HandleBirdButton;
            CoinButton.clickable.clicked -= HandleCoinButton;
            LiraButton.clickable.clicked -= HandleLiraButton;
            CoffeeButton.clickable.clicked -= HandleCoffeeButton;
            TuctucButton.clickable.clicked -= HandleTuctucButton;
            ConfirmButton.clickable.clicked -= HandleConfirmButton;
        }

        private void SetButtonColor(ButtonState state, Button button)
        {
            // Enable the class related to the state and disable the other classes.
            // TODO: There is abug with the flickering default when setting to choosing
            System.Collections.IList list = Enum.GetValues(typeof(ButtonState));
            for (int i = 0; i < list.Count; i++)
            {
                ButtonState s = (ButtonState)list[i];
                button.parent.EnableInClassList(_stateClasses[s], s == state);
            }
        }

        public void SetDefault(CharactersEnum character) => SetCharacterState(character, ButtonState.Default);

        public void SetChoosing(CharactersEnum character) => SetCharacterState(character, ButtonState.Choosing);

        private void SetCharacterState(CharactersEnum character, ButtonState state)
        {
            switch (character)
            {
                case CharactersEnum.Avocado:
                    SetButtonColor(state, AvocadoButton);
                    break;
                case CharactersEnum.Bird:
                    SetButtonColor(state, BirdButton);
                    break;
                case CharactersEnum.Coin:
                    SetButtonColor(state, CoinButton);
                    break;
                case CharactersEnum.Lira:
                    SetButtonColor(state, LiraButton);
                    break;
                case CharactersEnum.Coffee:
                    SetButtonColor(state, CoffeeButton);
                    break;
                case CharactersEnum.Tuctuc:
                    SetButtonColor(state, TuctucButton);
                    break;
            }
        }

        private void HandleButtonClick(UIRequestType uIRequestType)
        {
            UIRequests.Enqueue(new UIRequest { Value = uIRequestType });
        }

        private void HandleAvocadoButton()
        {
            HandleButtonClick(UIRequestType.AvocadoButton);
        }

        private void HandleBirdButton()
        {
            HandleButtonClick(UIRequestType.BirdButtoon);
        }

        private void HandleCoinButton()
        {
            HandleButtonClick(UIRequestType.CoinButton);
        }

        private void HandleLiraButton()
        {
            HandleButtonClick(UIRequestType.LiraButton);
        }

        private void HandleCoffeeButton()
        {
            HandleButtonClick(UIRequestType.CoffeButton);
        }

        private void HandleTuctucButton()
        {
            HandleButtonClick(UIRequestType.TuctucButton);
        }

        private void HandleConfirmButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.CharacterSelectConfirmButton });
        }
    }
}
