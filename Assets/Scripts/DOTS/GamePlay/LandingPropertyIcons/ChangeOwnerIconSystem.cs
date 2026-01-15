using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.LandingPropertyIcons
{
    [BurstCompile]
    public partial struct ChangeOwnerIconSystem : ISystem
    {
        public BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;
        public ComponentLookup<OwnerIconTag> ownerIconsLookup;
        public ComponentLookup<CharactersEnumComponent> characterEnumLookup;
        public ComponentLookup<UVOffsetOverride> uvOffsetOverrideLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CharacterNametoIconUVMapBlobReference>();
            state.RequireForUpdate<OwnerByEntityComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<OwnerIconTag>();
            state.RequireForUpdate<CharactersEnumComponent>();
            state.RequireForUpdate<UVOffsetOverride>();

            linkedEntityGroupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>();
            ownerIconsLookup = SystemAPI.GetComponentLookup<OwnerIconTag>();
            characterEnumLookup = SystemAPI.GetComponentLookup<CharactersEnumComponent>();
            uvOffsetOverrideLookup = SystemAPI.GetComponentLookup<UVOffsetOverride>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            linkedEntityGroupLookup.Update(ref state);
            ownerIconsLookup.Update(ref state);
            characterEnumLookup.Update(ref state);
            uvOffsetOverrideLookup.Update(ref state);

            var characterToIcon = SystemAPI.GetSingleton<CharacterNametoIconUVMapBlobReference>();

            foreach (var (owner, _, entity) in SystemAPI.Query<RefRO<OwnerByEntityComponent>, RefRO<PropertySpaceTag>>().WithEntityAccess().WithChangeFilter<OwnerComponent>())
            {
                if (owner.ValueRO.Entity != Entity.Null)
                {
                    var linkedEntities = linkedEntityGroupLookup[entity];
                    foreach (var linkedEntity in linkedEntities)
                    {
                        if (ownerIconsLookup.HasComponent(linkedEntity.Value))
                        {
                            var characterEnum = characterEnumLookup[owner.ValueRO.Entity].Value;
                            var uvOffset = characterToIcon.Reference.Value.array[(int)characterEnum];
                            var ownerIconEntity = linkedEntity.Value;
                            uvOffsetOverrideLookup.GetRefRW(ownerIconEntity).ValueRW.Value = uvOffset;
                        }
                    }
                }
            }
        }
    }
}
