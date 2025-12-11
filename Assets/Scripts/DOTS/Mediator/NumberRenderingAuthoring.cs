using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Mediator
{
    // Attached to the place where the quads are going to spawn
    public class NumberRenderingAuthoring : MonoBehaviour
    {
        public class NumberRenderingBaker : Baker<NumberRenderingAuthoring>
        {
            public override void Bake(NumberRenderingAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddBuffer<QuadDataBuffer>(entity);
            }
        }
    }

    public struct QuadDataBuffer : IBufferElementData
    {
        public float3 Position;
        public float2 UV0;
        public float2 UV1;
    }
}
