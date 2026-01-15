using TitleScreen.NetworkUI.Components;
using TitleScreen.NetworkUI.Panels;
using TitleScreen.NetworkUI.Systems;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HighlightPrepickedCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuPanelsComponent>();
            state.RequireForUpdate<PrepickedCharacter>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var panels = SystemAPI.ManagedAPI.GetSingleton<GameMenuPanelsComponent>();
            var panelLookup = panels.PanelLookup;

            foreach (var prepickedCharacter in SystemAPI.Query<RefRO<PrepickedCharacter>>())
            {
                if (panelLookup != null)
                {
                    var characterSelectPanel = panelLookup[GameMenuPhase.CharacterSelect] as CharacterSelectPanel;
                    var character = prepickedCharacter.ValueRO.Character;

                    if (prepickedCharacter.ValueRO.PrePicked)
                    {
                        characterSelectPanel.SetChoosing(character);
                    }
                    if (!prepickedCharacter.ValueRO.PrePicked)
                    {
                        characterSelectPanel.SetDefault(character);
                    }
                }
            }
        }
    }
}
