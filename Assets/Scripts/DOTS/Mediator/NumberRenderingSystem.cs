using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
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

            for (int i = 0; i < 1025; i++)
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
        }
    }

    public struct QuadUVs : IComponentData
    {
        public float2 Offset;
    }

    [MaterialProperty("_UVOffset")]
    public struct UVOffsetOverride : IComponentData
    {
        public float2 Value;
    }
}
