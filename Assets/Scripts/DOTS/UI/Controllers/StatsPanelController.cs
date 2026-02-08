using System;
using System.Collections.Generic;
using System.Linq;
using DOTS.UI.Panels;
using DOTS.UI.Utilities;
using TitleScreen.UI.GameMenu.Controllers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.UI.Controllers
{
    public struct StatsPanelContext
    {
        public FixedString64Bytes Name;
        public FixedString64Bytes Money;
    }

    public class StatsPanelController
    {
        public Dictionary<string, PlayerNameMoneyPanel> StatsPanelRegistry { get; private set; }

        public FixedString64Bytes CurrentPlayerName { get; private set; }
        // Use the list wherever order is need.
        public StatsPanelContext Context { get; set; }
        public VisualElement SmallPanelsContainer { get; private set; }
        private readonly List<PlayerNameMoneyPanel> _statsPanels;
        private readonly StatsPanelsPositionsCalculator _statsPanelsPositionsCalculator;
        private readonly SelectionHighlighter<PlayerNameMoneyPanel> _selectionHighlighter;
        private readonly Dictionary<FixedString64Bytes, Sprite> _characterSpriteRegistry;

        public StatsPanelController(VisualElement smallPanelsContainer, StatsPanelContext context, Dictionary<FixedString64Bytes, Sprite> characterSpritesRegistry)
        {
            SmallPanelsContainer = smallPanelsContainer;
            SmallPanelsContainer.style.visibility = Visibility.Hidden;

            _characterSpriteRegistry = characterSpritesRegistry;
            _selectionHighlighter = new(HighlightActivePanel, DisableHighlightActivePanel);
            _statsPanelsPositionsCalculator = new(SmallPanelsContainer);
            _statsPanels = new();

            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        private void HighlightActivePanel(PlayerNameMoneyPanel panel) => panel.HighlightActivePlayerPanel();
        private void DisableHighlightActivePanel(PlayerNameMoneyPanel panel) => panel.DisableHighlightActivePlayerPanel();

        public void SetupPanels(IReadOnlyList<string> orderedNames)
        {
            foreach (var name in orderedNames)
            {
                var panel = CreatePanel();
                StatsPanelRegistry.TryAdd(name, panel);
                Debug.Log($"[InitializeStatsPanelSystem] | registering : {name}");
            }

            _statsPanels.Clear();
            foreach (var name in orderedNames)
            {
                StatsPanelRegistry.TryGetValue(name, out var panel);
                _statsPanels.Add(panel);
                SmallPanelsContainer.Add(panel.Root);
            }
        }

        private PlayerNameMoneyPanel CreatePanel()
        {
            var tree = Resources.Load<VisualTreeAsset>("PlayerNameMoneyPanel");
            VisualElement playerNameMoneyPanelElement = tree.Instantiate();
            PlayerNameMoneyPanel panel = new(playerNameMoneyPanelElement);
            return panel;
        }

        public bool AllPanelsOnScreen
        {
            get
            {
                foreach (var panel in _statsPanels)
                {
                    if (!panel.IsOnScreen)
                        return false;
                }
                return true;
            }
        }

        public void InitializePanel()
        {
            UnityEngine.Debug.Log($"[StatsPanelController] | Initializing Panel: {Context.Name.ToString()}");
            StatsPanelRegistry.TryGetValue(Context.Name.ToString(), out var panel);
            var sprite = _characterSpriteRegistry[Context.Name.ToString()];
            _statsPanelsPositionsCalculator.AddPanel(panel.Root);
            panel.SetSprite(sprite);
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }

        ///<summary>
        ///Set the initialize position of the stats panels, including the current player's panel.
        ///</summary>
        public void SetPanelsInitialPositions()
        {
            _statsPanelsPositionsCalculator.CalculatePositions();
            int idx = _statsPanels.Count - 1;
            foreach (var panel in _statsPanels)
            {
                UnityEngine.Debug.Log($"[StatsPanelController] | panel Width: {panel.Root.resolvedStyle.width}");
                if (idx == _statsPanels.Count - 1)
                    TranslatePanel(panel, panel.Root, _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition);
                else
                    TranslatePanel(panel, idx, _statsPanelsPositionsCalculator.GetPanelPosition);
                idx--;
            }

            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        public void TranslateAllPanels()
        {
            // get the last panel.
            int idx = _statsPanels.Count - 1;
            foreach (var panel in _statsPanels)
            {
                int capturedIndex = idx;
                panel.Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
                // if we are the first panel move to the back
                if (idx == 0)
                {
                    panel.Root.style.transitionDuration = new List<TimeValue> { new(0f, TimeUnit.Second) };
                    panel.Root.style.translate = new Translate(350, 0);
                    panel.Root.schedule.Execute((_) =>
                    {
                        TranslatePanel(panel, capturedIndex, _statsPanelsPositionsCalculator.GetPanelPosition);
                    }).ExecuteLater(0);
                }

                if (capturedIndex == _statsPanels.Count - 1)
                {
                    panel.Root.schedule.Execute((_) =>
                    {
                        TranslatePanel(panel, panel.Root, _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition);
                    }).ExecuteLater(0);
                }
                if ((capturedIndex != _statsPanels.Count - 1) && (capturedIndex != 0))
                {
                    panel.Root.schedule.Execute((_) =>
                    {
                        TranslatePanel(panel, capturedIndex, _statsPanelsPositionsCalculator.GetPanelPosition);
                    }).ExecuteLater(0);
                }
                idx--;
            }

            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        private void TranslatePanel<T>(PlayerNameMoneyPanel panel, T value, Func<T, OffsetFromTopRight> GetPosition)
        {
            OffsetFromTopRight position = GetPosition(value);
            panel.Root.style.translate = new Translate(-position.Right, position.Top);
        }

        public void ShiftPanelsRegistry()
        {
            UnityEngine.Debug.Log($"[StatsPanelController] | shifting panels");
            var entries = _statsPanels;
            if (entries.Count == 0)
                return;

            var firstEntry = entries.First();
            entries.RemoveAt(0);
            entries.Insert(entries.Count, firstEntry);
        }

        public void HighlightPanel(int idx)
        {
            var panel = _statsPanels[idx];
            _selectionHighlighter.Select(panel);
        }
        public void HighlightPanel(FixedString64Bytes characterName)
        {
            var panel = StatsPanelRegistry[characterName.ToString()];
            UnityEngine.Debug.Log($"[StatsPanelController] | Selecting panel for: {characterName.ToString()}");
            _selectionHighlighter.Select(panel);
        }

        public PlayerNameMoneyPanel GetHighlightedPanel()
        {
            return _selectionHighlighter.GetCurrent();
        }

        public void TranslatePanelsAndShiftRegistry()
        {
            TranslateAllPanels();
            ShiftPanelsRegistry();
        }

        public void Update()
        {
            UnityEngine.Debug.Log($"[StatsPanelController] | Registry size: {StatsPanelRegistry.Count}");

            // TODO: There is an error here. the name is not found.
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            UnityEngine.Debug.Log($"[StatsPanelController] | Updating panel: {Context.Name}");
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }
    }
}
