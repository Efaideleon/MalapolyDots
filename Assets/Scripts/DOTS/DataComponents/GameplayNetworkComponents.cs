using Unity.Entities;

namespace DOTS.DataComponents
{ 
    public struct NetworkRoleTypeComponent : IComponentData
    {
        public NetworkRole Value;
    }

    public enum NetworkRole
    {
        None,
        Host,
        Client
    }
}
