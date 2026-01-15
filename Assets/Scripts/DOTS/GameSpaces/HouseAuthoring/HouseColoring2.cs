using Unity.Entities;
using Unity.Rendering;

namespace DOTS.GameSpaces.HouseAuthoring
{
    [MaterialProperty("_building_2_slider")]
    public struct HouseColoring2 : IComponentData
    {
        public float Value;
    };
}
