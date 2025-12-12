using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

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
            var pos = SystemAPI.GetComponent<LocalToWorld>(entity);
            var meshDesc = new RenderMeshDescription(ShadowCastingMode.Off, false);
            var renderMeshArray = new RenderMeshArray(new[] { assets.material }, new[] { assets.mesh });


            for (int i = 0; i < 900; i++)
            {
                var quadEntity = state.EntityManager.CreateEntity();
                RenderMeshUtility.AddComponents(
                        quadEntity,
                        state.EntityManager,
                        meshDesc,
                        renderMeshArray,
                        MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
                );
                state.EntityManager.AddComponentData(quadEntity, new UVOffsetOverride
                {
                    Value = new float2(UnityEngine.Random.Range(0, 10f), UnityEngine.Random.Range(0, 10f))
                });
                state.EntityManager.AddComponentData(quadEntity, LocalTransform.FromPosition(
                    pos.Position + new float3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0, 1f), 0f))
                );
            }

            state.Enabled = false;


            // state.EntityManager.Instantiate(quadEntity);
            // var entity = SystemAPI.GetSingletonEntity<QuadDataBuffer>();
            // var pos = SystemAPI.GetComponent<LocalToWorld>(entity).Position;
            //
            // QuadData q1 = new() { UV0 = new float2(0, 0), UV1 = new float2(0.2f, 1f) };
            // QuadData q2 = new() { UV0 = new float2(0.2f, 0f), UV1 = new float2(0.4f, 1f) };
            // QuadData q3 = new() { UV0 = new float2(0.4f, 0f), UV1 = new float2(0.8f, 1f) };
            //
            // var block1 = new MaterialPropertyBlock();
            // var block2 = new MaterialPropertyBlock();
            // var block3 = new MaterialPropertyBlock();
            //
            // block1.SetVector("_UV0", new Vector4(q1.UV0.x, q1.UV0.y, 0, 0));
            //
            // block1.SetVector("_UV1", new Vector4(q1.UV1.x, q1.UV1.y, 0, 0));
            //
            // block2.SetVector("_UV0", new Vector4(q2.UV0.x, q2.UV0.y, 0, 0));
            // block2.SetVector("_UV1", new Vector4(q2.UV1.x, q2.UV1.y, 0, 0));
            //
            // block3.SetVector("_UV0", new Vector4(q3.UV0.x, q3.UV0.y, 0, 0));
            // block3.SetVector("_UV1", new Vector4(q3.UV1.x, q3.UV1.y, 0, 0));

            // for (int i = 0; i < 300; i++)
            // {
            //     Graphics.DrawMesh
            //         (
            //          assets.mesh,
            //          float4x4.Translate(pos),
            //          assets.material,
            //          0,
            //          null,
            //          0,
            //          block1
            //         );
            //
            //     var offset = new float3(1, 0, 0);
            //     Graphics.DrawMesh
            //         (
            //          assets.mesh,
            //          float4x4.Translate(pos + offset),
            //          assets.material,
            //          0,
            //          null,
            //          0,
            //          block2
            //         );
            //
            //     var offset2 = new float3(2, 0, 0);
            //     Graphics.DrawMesh
            //         (
            //          assets.mesh,
            //          float4x4.Translate(pos + offset2),
            //          assets.material,
            //          0,
            //          null,
            //          0,
            //          block3
            //         );
            // }
        }
    }

    public struct QuadUVs : IComponentData
    {
        public float2 Offset;
    }

    public struct QuadData
    {
        public float2 UV0;
        public float2 UV1;
    }


    [MaterialProperty("_UVOffset")]
    public struct UVOffsetOverride : IComponentData
    {
        public float2 Value;
    }
}
