using DOTS.DataComponents;
using DOTS.GameSpaces;
using DOTS.GameSpaces.HouseAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
    public partial class PropertyLodMaterialSystem : SystemBase
    {
        protected override void OnCreate() => RequireForUpdate<PropertyLodSource>();

        protected override void OnUpdate()
        {
            Dependency.Complete();
            foreach (var (source, renderer) in SystemAPI.Query<RefRO<PropertyLodSource>>().WithEntityAccess())
            {
                var property = source.ValueRO.Value;
                if (!EntityManager.HasComponent<MaterialOverrideColorSlider>(property)) continue;
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<MaterialOverrideColorSlider>(property));
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<BlinkingFlagMaterialOverride>(property));
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<HouseColoring1>(property));
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<HouseColoring2>(property));
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<HouseColoring3>(property));
                EntityManager.SetComponentData(renderer, EntityManager.GetComponentData<HouseColoring4>(property));
            }
        }
    }
}
