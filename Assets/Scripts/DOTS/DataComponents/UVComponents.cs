using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;

namespace DOTS.DataComponents
{
    [GhostComponent]
    [MaterialProperty("_UVOffset")]
    public struct UVOffsetOverride : IComponentData
    {
        [GhostField]
        public float2 Value;
    }

    [GhostComponent]
    [MaterialProperty("_UVScale")]
    public struct UVScaleOverride : IComponentData
    {
        [GhostField]
        public float2 Value;
    }
}
