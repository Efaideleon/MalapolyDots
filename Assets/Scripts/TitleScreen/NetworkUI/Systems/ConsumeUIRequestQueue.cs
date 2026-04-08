using TitleScreen.NetworkUI.Components;
using Unity.Entities;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ConsumeUIRequestQueue : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuUIRequests>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.ManagedAPI.TryGetSingleton<GameMenuUIRequests>(out var GameMenuUIRequests))
                return;

            var ecb = GetECB(ref state);

            while (GameMenuUIRequests.UIRequests.TryDequeue(out var request))
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent<UIEvent>(entity);
                switch (request.Value)
                {
                    case UIRequestType.MainMenuHost:
                        ecb.AddComponent<MainMenuHostClickEvent>(entity);
                        break;
                    case UIRequestType.MainMenuJoin:
                        ecb.AddComponent<MainMenuJoinClickEvent>(entity);
                        break;
                    case UIRequestType.HostSetupHost:
                        ecb.AddComponent<HostSetupHostClickEvent>(entity);
                        break;
                    case UIRequestType.JoinSetupJoin:
                        ecb.AddComponent<JoinSetupJoinClickEvent>(entity);
                        break;
                    case UIRequestType.LobbyStartButton:
                        ecb.AddComponent<LobbyStartClickEvent>(entity);
                        break;
                    case UIRequestType.Back:
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | BackButton clicked");
                        ecb.AddComponent<BackButtonClickEvent>(entity);
                        break;
                    

                    case UIRequestType.AvocadoButton:
                        ecb.AddComponent<AvocadoClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | AvocadoClicked");
                        break;
                    case UIRequestType.BirdButtoon:
                        ecb.AddComponent<BirdClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | BirdClicked");
                        break;
                    case UIRequestType.CoinButton:
                        ecb.AddComponent<CoinClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | CoinClicked");
                        break;
                    case UIRequestType.LiraButton:
                        ecb.AddComponent<LiraClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | LiraClicked");
                        break;
                    case UIRequestType.CoffeButton:
                        ecb.AddComponent<CoffeeClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | CoffeeClicked");
                        break;
                    case UIRequestType.TuctucButton:
                        ecb.AddComponent<TuctucClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | TuctucClicked");
                        break;
                    case UIRequestType.CharacterSelectConfirmButton:
                        ecb.AddComponent<CharacterSelectConfirmClickEvent>(entity);
                        UnityEngine.Debug.Log($"[ConsumeUIRequestQueue] | Confirm clicked");
                        break;
                }
            }
        }

        private readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}
