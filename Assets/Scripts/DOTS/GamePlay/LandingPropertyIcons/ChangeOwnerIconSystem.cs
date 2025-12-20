using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.GamePlay.LandingPropertyIcons
{
    public partial struct ChangeOwnerIconSystem : ISystem
    {
        public BufferLookup<LinkedEntityGroup> linkedEntityGroupLookup;
        public ComponentLookup<OwnerIconTag> ownerIconsLookup;

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
        }

        public void OnUpdate(ref SystemState state)
        {
            linkedEntityGroupLookup.Update(ref state);
            ownerIconsLookup.Update(ref state);

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
                            var characterEnum = SystemAPI.GetComponent<CharactersEnumComponent>(owner.ValueRO.Entity).Value;
                            var uvOffset = characterToIcon.Reference.Value.array[(int)characterEnum];
                            var ownerIconEntity = linkedEntity.Value;
                            SystemAPI.GetComponentRW<UVOffsetOverride>(ownerIconEntity).ValueRW.Value = uvOffset;
                            UnityEngine.Debug.Log($"[ChangeOwnerIconSystem] | Changing Icon offset {uvOffset}!");
                        }
                    }
                }
            }
        }
    }
}
