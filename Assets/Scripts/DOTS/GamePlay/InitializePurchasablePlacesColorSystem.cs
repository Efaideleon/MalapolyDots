using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.GamePlay
{
    ///<summary>
    ///This system initializes the material coloring field for each property to 40.
    ///Intializing ColorSlider to 40 makes the coloring start faster.
    ///</summary>
    public partial struct InitializePurchasablePlacesColorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MaterialOverrideColorSlider>();
            state.RequireForUpdate<OwnerComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            foreach (var (materialColorSlider, _) in 
                    SystemAPI.Query<
                        RefRW<MaterialOverrideColorSlider>,
                        RefRO<PropertySpaceTag>
                    >()) 
            {
                ref var colorSliderRW = ref materialColorSlider.ValueRW;
                colorSliderRW.Value = 40;
            }
        }
    }
}
