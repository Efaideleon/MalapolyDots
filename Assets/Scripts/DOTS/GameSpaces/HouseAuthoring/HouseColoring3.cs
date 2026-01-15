using Unity.Entities;
using Unity.Rendering;

namespace DOTS.GameSpaces.HouseAuthoring
{
    [MaterialProperty("_building_3_slider")]
    public struct HouseColoring3 : IComponentData
    {
        public float Value;
    };
}
