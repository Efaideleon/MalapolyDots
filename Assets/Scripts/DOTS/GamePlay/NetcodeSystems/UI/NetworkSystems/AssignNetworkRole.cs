using DOTS.DataComponents;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct AssignNetworkRole : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UIEvent>();
            state.RequireForUpdate<NetworkRoleTypeComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool isHostClicked = false;
            bool isJoinClicked = false;

            foreach (var evt in SystemAPI.Query<RefRO<HostSetupHostClickEvent>>())
            {
                isHostClicked = true;
            }

            foreach (var evt in SystemAPI.Query<RefRO<JoinSetupJoinClickEvent>>())
            {
                isJoinClicked = true;
            }

            if (isHostClicked || isJoinClicked)
            {
                NetworkRole networkRole = isHostClicked ? NetworkRole.Host : NetworkRole.Client;

                    SystemAPI.GetSingletonRW<NetworkRoleTypeComponent>().ValueRW.Value = networkRole;
            }
        }
    }
}
