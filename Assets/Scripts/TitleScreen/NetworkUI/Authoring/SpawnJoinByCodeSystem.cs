using TitleScreen.NetworkUI.Authoring;
using Unity.Entities;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnJoinByCodeSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JoinByCodeUIReference>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var gameMenuRef = SystemAPI.ManagedAPI.GetSingleton<JoinByCodeUIReference>();
            var gameMenuGO = gameMenuRef.uiDocumentGO;
            if (gameMenuGO == null)
                return;

            UnityEngine.Debug.Log($"[SpawnGameMenuUISystem] | spawning ui doc");
            // Create the ui toolkit game menu.
            var uiGameObject = UnityEngine.Object.Instantiate(gameMenuRef.uiDocumentGO);
            if (!uiGameObject.TryGetComponent<UIDocument>(out var uiDocument))
            {
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
