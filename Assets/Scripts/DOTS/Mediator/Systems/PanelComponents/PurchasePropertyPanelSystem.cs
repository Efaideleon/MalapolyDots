using Unity.Collections;
using Unity.Entities;
namespace DOTS.Mediator.Systems.PanelComponents
{
    public partial struct PurchasePropertyPanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateArchetype
            (
                // typeof(),
                // typeof()
            );
        }
        public void OnUpdate(ref SystemState state)
        {
        }
    }

    public struct TitleComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }

    public struct PriceComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }
}
