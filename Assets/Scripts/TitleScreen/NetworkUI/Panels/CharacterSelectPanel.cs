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
        private CharactersEnum? pendingCharacter;
        private float pendingSelectionExpiresAt;
        private const float SelectionResponseTimeout = 5f;

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
            pendingCharacter = null;
        }

        private void SubscribeEvents()
        {
            AvocadoButton.clickable.clicked += HandleAvocadoButton;
            BirdButton.clickable.clicked += HandleBirdButton;
            CoinButton.clickable.clicked += HandleCoinButton;
            LiraButton.clickable.clicked += HandleLiraButton;
            CoffeeButton.clickable.clicked += HandleCoffeeButton;
            TuctucButton.clickable.clicked += HandleTuctucButton;
            ConfirmButton.clickable.clicked += HandleConfirmButton;
        }

        private void UnSubscribeEvents()
        {
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
            System.Collections.IList list = Enum.GetValues(typeof(ButtonState));
            for (int i = 0; i < list.Count; i++)
            {
                ButtonState s = (ButtonState)list[i];
                button.parent.EnableInClassList(_stateClasses[s], s == state);
            }
        }

        public void SetDefault(CharactersEnum character)
        {
            if (pendingCharacter == character)
            {
                if (UnityEngine.Time.unscaledTime < pendingSelectionExpiresAt) return;
                pendingCharacter = null;
            }
            SetCharacterState(character, ButtonState.Default);
        }

        public void SetChoosing(CharactersEnum character)
        {
            if (pendingCharacter == character) pendingCharacter = null;
            SetCharacterState(character, ButtonState.Choosing);
        }

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

        private void HandleButtonClick(UIRequestType uIRequestType, CharactersEnum character)
        {
            pendingCharacter = character;
            pendingSelectionExpiresAt = UnityEngine.Time.unscaledTime + SelectionResponseTimeout;
            SetCharacterState(character, ButtonState.Choosing);
            UIRequests.Enqueue(new UIRequest { Value = uIRequestType });
        }

        private void HandleAvocadoButton()
        {
            HandleButtonClick(UIRequestType.AvocadoButton, CharactersEnum.Avocado);
        }

        private void HandleBirdButton()
        {
            HandleButtonClick(UIRequestType.BirdButtoon, CharactersEnum.Bird);
        }

        private void HandleCoinButton()
        {
            HandleButtonClick(UIRequestType.CoinButton, CharactersEnum.Coin);
        }

        private void HandleLiraButton()
        {
            HandleButtonClick(UIRequestType.LiraButton, CharactersEnum.Lira);
        }

        private void HandleCoffeeButton()
        {
            HandleButtonClick(UIRequestType.CoffeButton, CharactersEnum.Coffee);
        }

        private void HandleTuctucButton()
        {
            HandleButtonClick(UIRequestType.TuctucButton, CharactersEnum.Tuctuc);
        }

        private void HandleConfirmButton()
        {
            UIRequests.Enqueue(new UIRequest { Value = UIRequestType.CharacterSelectConfirmButton });
        }

        private void HandleBackButton()
        {
            UnityEngine.Debug.Log("[CharacterSelectPanel] | BackButton pressed.");
        }
    }
}
