using Unity.Entities;
using Unity.Rendering;

namespace DOTS.GameSpaces.HouseAuthoring
{
    [MaterialProperty("_building_1_slider")]
    public struct HouseColoring1 : IComponentData
    {
        public float Value;
    };
}
