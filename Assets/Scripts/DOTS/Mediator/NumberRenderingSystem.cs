using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.Mediator
{
    public partial struct NumberRenderingSystem : ISystem
    {
        private const int Number = 123456789;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AssetsMaterial>();
            state.RequireForUpdate<QuadDataBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // foreach (var (quadDataBuffer, localTransform) in SystemAPI.Query<DynamicBuffer<QuadDataBuffer>, RefRO<LocalTransform>>())
            // {
            //     foreach (var quadData in quadDataBuffer)
            //     {
            //         quadData.UV0 = 
            //         quadData.UV1 = 
            //         quadData.Position = 
            //     }
            // }
            //
            var assets = SystemAPI.ManagedAPI.GetSingleton<AssetsMaterial>();

            var entity = SystemAPI.GetSingletonEntity<QuadDataBuffer>();
            var pos = SystemAPI.GetComponent<LocalToWorld>(entity).Position;

            QuadData q1 = new() { UV0 = new float2(0, 0), UV1 = new float2(0.5f, 1f) };
            QuadData q2 = new() { UV0 = new float2(0, 0.5f), UV1 = new float2(1, 1) };

            var block1 = new MaterialPropertyBlock();
            var block2 = new MaterialPropertyBlock();

            block1.SetVector("_UV0", new Vector4(q1.UV0.x, q1.UV0.y, 0, 0));
            block1.SetVector("_UV1", new Vector4(q1.UV1.x, q1.UV1.y, 0, 0));

            block2.SetVector("_UV0", new Vector4(q2.UV0.x, q2.UV0.y, 0, 0));
            block2.SetVector("_UV1", new Vector4(q2.UV1.x, q2.UV1.y, 0, 0));

            Graphics.DrawMesh
            (
                assets.mesh,
                float4x4.Translate(pos),
                assets.material,
                0,
                null,
                0,
                block1
            );

            var offset = new float3(0.2f, 0, 0);
            Graphics.DrawMesh
            (
                assets.mesh,
                float4x4.Translate(pos + offset),
                assets.material,
                0,
                null,
                0,
                block2
            );
        }
    }

    public struct QuadData
    {
        public float2 UV0;
        public float2 UV1;
    }
}
