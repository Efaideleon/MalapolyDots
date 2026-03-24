using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    ///<summary>
    ///This stores the uvoffset that represents each digit in the quad.
    [GhostComponent]
    public struct QuadDataBuffer : IBufferElementData
    {
        [GhostField]
        public float2 UVOffset;
    }
}
