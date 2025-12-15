using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace DOTS.Mediator
{
    public partial struct DisplayPriceOnQuadsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<QuadDataBuffer>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<QuadsEntitiesBuffer>();
        }
        public void OnUpdate(ref SystemState state)
        {
            // Run this in a IJobEntity
            foreach (var (quadOffsetBuffer, quadEntitiesBuffer) in
                    SystemAPI.Query<
                    DynamicBuffer<QuadDataBuffer>,
                    DynamicBuffer<QuadsEntitiesBuffer>
                    >()
                    .WithChangeFilter<QuadDataBuffer>())
            {
                int usedCount = math.min(quadOffsetBuffer.Length, quadEntitiesBuffer.Length);

                for (int i = 0; i < usedCount; i++)
                {
                    if (i < quadEntitiesBuffer.Length)
                    {
                        // Enable used quads.
                        var quadEntity = quadEntitiesBuffer[i].Entity;
                        if (SystemAPI.HasComponent<MaterialMeshInfo>(quadEntity))
                        {
                            SystemAPI.SetComponentEnabled<MaterialMeshInfo>(quadEntity, true);
                        }
                        // Set number on quad.
                        var offset = SystemAPI.GetComponentRW<UVOffsetOverride>(quadEntity);
                        offset.ValueRW.Value = quadOffsetBuffer[i].UVOffset;
                    }
                }
                // Disable the renderer for remaining quad entities.
                for (int i = usedCount; i < quadEntitiesBuffer.Length; i++)
                {
                    var quadEntity = quadEntitiesBuffer[i].Entity;
                    if (SystemAPI.HasComponent<MaterialMeshInfo>(quadEntity))
                    {
                        SystemAPI.SetComponentEnabled<MaterialMeshInfo>(quadEntity, false);
                    }
                }
            }
        }
    }
}
