using System;
using System.Collections.Generic;
using System.Linq;
using DOTS.UI.Panels;
using DOTS.UI.Utilities;
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
        public Dictionary<FixedString64Bytes, PlayerNameMoneyPanel> StatsPanelRegistry { get; private set; }
        public VisualElement SmallPanelsContainer { get; private set; }
        private readonly List<PlayerNameMoneyPanel> _statsPanels;
        private readonly StatsPanelsPositionsCalculator _statsPanelsPositionsCalculator;
        private readonly Dictionary<FixedString64Bytes, Sprite> _characterSpriteRegistry;

        public StatsPanelController(VisualElement smallPanelsContainer, Dictionary<FixedString64Bytes, Sprite> characterSpritesRegistry)
        {
            SmallPanelsContainer = smallPanelsContainer;
            SmallPanelsContainer.style.visibility = Visibility.Hidden;

            _characterSpriteRegistry = characterSpritesRegistry;
            _statsPanelsPositionsCalculator = new(SmallPanelsContainer);
            _statsPanels = new();

            StatsPanelRegistry = new Dictionary<FixedString64Bytes, PlayerNameMoneyPanel>();
        }

        /// <summary>
        /// Creates a panel and registers to a name from the orderedNames into a dictionary.
        /// Also Adds the panel into a VisualElement contianer.
        /// </summary>
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

        /// <summary>
        /// Check if all the stats panels width has been resolved (is on screen).
        /// </summary>
        /// <returns> True if width has been resolved for ALL stats panels.</returns>
        public bool IsWidthResolvedForAllPanels
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

        public void InitializePanel(StatsPanelContext context)
        {
            if (StatsPanelRegistry.TryGetValue(context.Name, out var panel))
            {
                if (_characterSpriteRegistry.TryGetValue(context.Name, out var sprite))
                {
                    panel.SetSprite(sprite);
                    panel.SetName(context.Name);
                    panel.SetMoney(context.Money);
                }
                else
                {
                    new Exception($"{context.Name} is not in the _characterSpriteRegistry. Can't load panel data.");
                }
            }
            else
            {
                new Exception($"{context.Name} is not in the StatsPanelRegistry, Can't initialize panel.");
            }
        }

        ///<summary>
        /// Set the initialize position of the stats panels, including the current player's panel.
        /// This method must be called after the width for the panels has been resolved.
        ///</summary>
        public void SetPanelsInitialPositions()
        {
            int numOfPanels = _statsPanels.Count;
            if (_statsPanels.Count == 0)
            {
                new ArgumentOutOfRangeException("_statsPanels is not initialized");
            }

            if (_statsPanels[0].Root.resolvedStyle.width == 0)
            {
                new Exception("Unresolved stats panels width.");
            }

            _statsPanels[0].HighlightPanel();
            _statsPanelsPositionsCalculator.CalculatePositions(_statsPanels[0].Root.resolvedStyle.width, numOfPanels);
            TranslateAllPanels();
            SmallPanelsContainer.style.visibility = Visibility.Visible;
        }

        public void TranslateAllPanels()
        {
            int numOfPanels = _statsPanels.Count;

            for (int i = 0; i < numOfPanels; i++)
            {
                var panel = _statsPanels[i];
                bool isLastPanel = i == numOfPanels - 1;

                if (isLastPanel)
                {
                    MovePanelToRightOffscreen(panel);
                }

                AnimatePanelTranslationByOrder(panel, i);
            }
        }

        private void MovePanelToRightOffscreen(PlayerNameMoneyPanel panel)
        {
            panel.Root.style.transitionDuration = new List<TimeValue> { new(0f, TimeUnit.Second) };
            panel.Root.style.translate = new Translate(350, 0);
        }

        private void AnimatePanelTranslationByOrder(PlayerNameMoneyPanel panel, int order)
        {
            panel.Root.schedule.Execute((_) =>
            {
                panel.Root.style.transitionDuration = new List<TimeValue> { new(1f, TimeUnit.Second) };
                TranslatePanel(panel, order);
            }).ExecuteLater(0);
        }

        private void TranslatePanel(PlayerNameMoneyPanel panel, int order)
        {
            OffsetFromTopRight position = _statsPanelsPositionsCalculator.GetPosition(order);
            panel.Root.style.translate = new Translate(-position.Right, position.Top);
        }

        public void ShiftPanels()
        {
            UnityEngine.Debug.Log($"[StatsPanelController] | shifting panels");
            if (_statsPanels.Count == 0)
                return;

            _statsPanels[0].DisableHighlightPanel();

            var firstEntry = _statsPanels.First();
            _statsPanels.RemoveAt(0);
            _statsPanels.Insert(_statsPanels.Count, firstEntry);

            _statsPanels[0].HighlightPanel();
        }

        public PlayerNameMoneyPanel GetHighlightedPanel()
        {
            if (_statsPanels.Count == 0)
            {
                new ArgumentOutOfRangeException("_statsPanels is empty. No currently highlighted panel. Call InitializePanel to load panels.");
            }
            return _statsPanels[0];
        }

        /// <summary>
        /// Updates the stats panel's context.
        /// </summary>
        /// <param name="context" The name of the panel to update and it's new content </param>
        public void Update(StatsPanelContext context)
        {
            if (StatsPanelRegistry.TryGetValue(context.Name, out var panel))
            {
                panel.SetName(context.Name);
                panel.SetMoney(context.Money);
            }
            else
            {
                new Exception($"{context.Name} is not a valid key for StatsPanelRegistry. Call InitializePanel() to load panels");
            }
        }
    }
}
