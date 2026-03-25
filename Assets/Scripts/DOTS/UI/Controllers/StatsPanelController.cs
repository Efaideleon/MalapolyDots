using System;
using System.Collections.Generic;
using System.Linq;
using DOTS.UI.Panels;
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
        public Dictionary<string, PlayerNameMoneyPanel> StatsPanelRegistry;
        public StatsPanelContext Context { get; set; }
        public VisualElement SmallPanelsContainer { get; private set; }
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
            StatsPanelRegistry = new Dictionary<string, PlayerNameMoneyPanel>();
            Context = context;
        }

        private void HighlightActivePanel(PlayerNameMoneyPanel panel) => panel.HighlightActivePlayerPanel(); 
        private void DisableHighlightActivePanel(PlayerNameMoneyPanel panel) => panel.DisableHighlightActivePlayerPanel();

        public void RegisterPanel(string character, PlayerNameMoneyPanel panel)
        {
            SmallPanelsContainer.Add(panel.Root);
            UnityEngine.Debug.Log($"[StatsPanelController] | registering panel width : {panel.Root.resolvedStyle.width}");
            StatsPanelRegistry.Add(character, panel);
        }

        public void InitializePanel()
        {
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            var sprite = _characterSpriteRegistry[Context.Name.ToString()];
            _statsPanelsPositionsCalculator.AddPanel(panel.Root);
            panel.SetSprite(sprite);
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }

        public void SetPanelsInitialPositions()
        {
            _statsPanelsPositionsCalculator.CalculatePositions();
            int idx = StatsPanelRegistry.Count - 1;
            foreach (var kvp in StatsPanelRegistry)
            {
                var panel = kvp.Value;
                UnityEngine.Debug.Log($"[StatsPanelController] | panel Width: {panel.Root.resolvedStyle.width}");
                if (idx == StatsPanelRegistry.Count - 1)
                    TranslatePanel(panel, panel.Root, _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition);
                else
                    TranslatePanel(panel, idx, _statsPanelsPositionsCalculator.GetPanelPosition);
                idx--;
            }

            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        public void TranslateAllPanels()
        {
            int idx = StatsPanelRegistry.Count - 1;
            foreach (var kvp in StatsPanelRegistry)
            {
                var panel = kvp.Value;
                int capturedIndex = idx;
                if (idx == 0)
                {
                    panel.Root.style.transitionDuration = new List<TimeValue> { new(0f, TimeUnit.Second) };
                    panel.Root.style.translate = new Translate(350, 0);
                    panel.Root.schedule.Execute((_) =>
                    {
                        panel.Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
                        TranslatePanel(panel, capturedIndex, _statsPanelsPositionsCalculator.GetPanelPosition);
                    }).ExecuteLater(0);
                }
                if (capturedIndex == StatsPanelRegistry.Count - 1)
                {
                    panel.Root.schedule.Execute((_) =>
                    {
                        panel.Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
                        TranslatePanel(panel, panel.Root, _statsPanelsPositionsCalculator.GetCurrentPlayerPanelPosition);
                    }).ExecuteLater(0);
                }
                if ((capturedIndex != StatsPanelRegistry.Count -1)&& (capturedIndex != 0))
                {
                    panel.Root.schedule.Execute((_) =>
                    {
                        panel.Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
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
            var entries = StatsPanelRegistry.ToList();
            if (entries.Count == 0)
                return;

            var firstEntry = entries.First();
            entries.RemoveAt(0);
            entries.Insert(entries.Count, firstEntry);

            StatsPanelRegistry = entries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void SelectPanel(int idx)
        {
            var panel = StatsPanelRegistry.Values.ToArray()[idx];
            _selectionHighlighter.Select(panel);
        }
        public void SelectPanel(FixedString64Bytes characterName)
        {
            var panel = StatsPanelRegistry[characterName.ToString()];
            _selectionHighlighter.Select(panel);
        }

        public void TranslatePanelsAndShiftRegistry()
        {
            TranslateAllPanels();
            ShiftPanelsRegistry();
        }

        public void Update()
        {
            var panel = StatsPanelRegistry[Context.Name.ToString()];
            UnityEngine.Debug.Log($"[StatsPanelController] | Updating panel: {Context.Name}");
            panel.UpdatePlayerNameLabelText(Context.Name.ToString());
            panel.UpdatePlayerMoneyLabelText(Context.Money.ToString());
        }
    }
}
