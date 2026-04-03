using Assets.Scripts.DOTS.Mediator.PriceTag.Authoring;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;

namespace DOTS.Mediator
{
    // TODO: this should run on the client.
    // get the data from each quad from the server, added to some ghost component and send it to here and chane the property of the ghost here.
    // in a ghost entity if some componenet are not ghosts, and they are changed in the server, it won't show on the client.
    // if change change those non ghost components in the client it will show on that client alone.

    // The entities should only be spawned on the client, not on the server.
    // this way we avoid having to make each number a ghost entity and send its information over the server.

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct DisplayPriceOnQuadsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<QuadDataBuffer>();
            state.RequireForUpdate<QuadEntityPrefab>();
            state.RequireForUpdate<QuadsEntitiesBuffer>();
            state.RequireForUpdate<QuadTag>();
            state.RequireForUpdate<PropertySpaceTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            // Run this in a IJobEntity
            // TODO: the get the UVOffsetData and the property id, then match that id, to the new entity with the quadEntitiesBuffer.
            // UVOffsetData data comes from the server.
            // quadEntitiesBuffer is loaded in the client.
            // We have to wait until all the QuadsEntitiesBuffer have been loaded.
            bool reDrawQuads = false;

            foreach (var _ in SystemAPI.Query<PriceTagTag>().WithChangeFilter<QuadsEntitiesBuffer>())
            {
                reDrawQuads = true;
            }

            foreach (var _ in SystemAPI.Query<RefRO<PropertySpaceTag>>().WithChangeFilter<QuadDataBuffer>())
            {
                reDrawQuads = true;
            }

            // Either when the quad data changes or when the new entities are create we have to re draw the data on them.
            if (reDrawQuads)
            {
                foreach (var (spaceID, UVOffsetData) 
                    in SystemAPI.Query<RefRO<SpaceIDComponent>, DynamicBuffer<QuadDataBuffer>>().WithAll<PropertySpaceTag>())
                {
                    // find a way to keep trying until the quad data buffer has entities.
                    // if after while there are no entities in the buffer print a log statement and stop trying.
                    foreach (var (quadID, quadEntitiesBuffer) 
                        in SystemAPI.Query<RefRO<SpaceIDComponent>, DynamicBuffer<QuadsEntitiesBuffer>>().WithAll<PriceTagTag>())
                    {
                        if (spaceID.ValueRO.Value == quadID.ValueRO.Value)
                        {

                            int numberOfQuads = quadEntitiesBuffer.Length;
                            int numberOfDigits = math.min(UVOffsetData.Length, numberOfQuads);

                            for (int i = 0; i < numberOfDigits; i++)
                            {
                                if (i < numberOfQuads)
                                {
                                    // Enable used quads.
                                    var quadEntity = quadEntitiesBuffer[i].Entity;
                                    if (SystemAPI.HasComponent<MaterialMeshInfo>(quadEntity))
                                    {
                                        SystemAPI.SetComponentEnabled<MaterialMeshInfo>(quadEntity, true);
                                    }
                                    // Set number on quad.
                                    var quadUVOffset = SystemAPI.GetComponentRW<UVOffsetOverride>(quadEntity);
                                    quadUVOffset.ValueRW.Value = UVOffsetData[i].UVOffset;
                                }
                            }
                            // Disable the renderer for remaining quad entities.
                            for (int i = numberOfDigits; i < numberOfQuads; i++)
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
        }
    }
}
