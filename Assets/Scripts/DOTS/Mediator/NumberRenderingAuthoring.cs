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
            }
        }
    }
}
