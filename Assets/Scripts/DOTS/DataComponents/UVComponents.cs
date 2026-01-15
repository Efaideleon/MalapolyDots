using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DOTS.DataComponents
{
    [MaterialProperty("_UVOffset")]
    public struct UVOffsetOverride : IComponentData
    {
        public float2 Value;
    }

    [MaterialProperty("_UVScale")]
    public struct UVScaleOverride : IComponentData
    {
        public float2 Value;
    }
}
